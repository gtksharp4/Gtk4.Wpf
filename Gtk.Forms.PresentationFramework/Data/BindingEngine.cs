using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Windows.Data
{
	/// <summary>
	/// Binding engine: OneWay, TwoWay, OneTime, OneWayToSource.
	/// Round 7: backs each binding with a live <see cref="BindingExpression"/>;
	/// adds RelativeSource resolution, StringFormat, FallbackValue, TargetNullValue,
	/// and UpdateSourceTrigger (PropertyChanged / Explicit).
	/// </summary>
	internal static class BindingEngine
	{
		// Per-element list of active records.
		static readonly ConditionalWeakTable<FrameworkElement, List<BindingRecord>> _bindings =
			new ConditionalWeakTable<FrameworkElement, List<BindingRecord>>();

		// Per-element map of DP → live BindingExpression, for GetBindingExpression.
		static readonly ConditionalWeakTable<FrameworkElement, Dictionary<DependencyProperty, BindingExpressionBase>> _expressions =
			new ConditionalWeakTable<FrameworkElement, Dictionary<DependencyProperty, BindingExpressionBase>>();

		/// <summary>
		/// Registers <paramref name="binding"/> on <paramref name="dp"/> of
		/// <paramref name="target"/> and applies it immediately.
		/// Returns the live <see cref="BindingExpressionBase"/>.
		/// </summary>
		internal static BindingExpressionBase? SetBinding(
			FrameworkElement target,
			DependencyProperty dp,
			BindingBase bindingBase)
		{
			var list = _bindings.GetOrCreateValue(target);

			int idx = list.FindIndex(r => r.TargetProperty == dp);
			if (idx >= 0)
			{
				list[idx].Deactivate();
				list.RemoveAt(idx);
			}

			// Remove old expression entry.
			var exprMap = _expressions.GetOrCreateValue(target);
			exprMap.Remove(dp);

			if (bindingBase is MultiBinding mb)
			{
				var mbExpr = MultiBindingEngine.SetBinding(target, dp, mb);
				exprMap[dp] = mbExpr;
				return mbExpr;
			}

			if (bindingBase is Binding binding)
			{
				var expr = new BindingExpression(target, dp, binding);
				var record = new BindingRecord(target, dp, binding, expr);
				expr.Record = record;
				list.Add(record);
				exprMap[dp] = expr;
				record.Activate();
				return expr;
			}

			return null;
		}

		/// <summary>Returns the live expression for <paramref name="dp"/>, or null.</summary>
		internal static BindingExpressionBase? GetBindingExpression(
			FrameworkElement target,
			DependencyProperty dp)
		{
			if (!_expressions.TryGetValue(target, out var map)) return null;
			map.TryGetValue(dp, out var expr);
			return expr;
		}

		/// <summary>Re-evaluates all bindings whose source is the inherited DataContext.</summary>
		internal static void RefreshBindings(FrameworkElement target)
		{
			if (!_bindings.TryGetValue(target, out var list)) return;
			foreach (var record in list)
			{
				if (!record.UsesDataContext) continue;
				record.Activate();
			}
		}

		// ── Path resolution ──────────────────────────────────────────────────────

		/// <summary>Resolves a dot-separated path on <paramref name="source"/>.</summary>
		internal static object? ResolvePath(object? source, string? path)
		{
			if (source == null) return null;
			if (string.IsNullOrEmpty(path)) return source;

			var current = source;
			foreach (var segment in path!.Split('.'))
			{
				if (current == null) return null;
				var prop = current.GetType().GetProperty(
					segment, BindingFlags.Public | BindingFlags.Instance);
				if (prop == null) return null;
				current = prop.GetValue(current);
			}
			return current;
		}

		/// <summary>
		/// Writes <paramref name="value"/> back through the dot-separated path on
		/// <paramref name="source"/>. Returns <c>true</c> on success.
		/// </summary>
		internal static bool WriteBackPath(object? source, string? path, object? value)
		{
			if (source == null) return false;
			if (string.IsNullOrEmpty(path)) return false;

			var current = source;
			var segments = path!.Split('.');
			for (int i = 0; i < segments.Length - 1; i++)
			{
				if (current == null) return false;
				var prop = current.GetType().GetProperty(segments[i],
					BindingFlags.Public | BindingFlags.Instance);
				if (prop == null) return false;
				current = prop.GetValue(current);
			}

			if (current == null) return false;
			var lastProp = current.GetType().GetProperty(
				segments[segments.Length - 1], BindingFlags.Public | BindingFlags.Instance);
			if (lastProp == null || !lastProp.CanWrite) return false;
			lastProp.SetValue(current, value);
			return true;
		}

		static object? ApplyStringFormat(string? fmt, object? value)
		{
			if (fmt == null) return value;
			string s = fmt.Contains("{0}") ? fmt : fmt + "{0}";
			return string.Format(CultureInfo.InvariantCulture, s, value);
		}

		// ── Inner record ────────────────────────────────────────────────────────

		internal sealed class BindingRecord
		{
			readonly WeakReference<FrameworkElement> _targetRef;
			readonly DependencyProperty _dp;
			readonly Binding _binding;
			readonly BindingExpression _expr;

			INotifyPropertyChanged? _inpcSource;
			string? _watchedProperty;
			bool _pushing;

			internal DependencyProperty TargetProperty => _dp;

			internal bool UsesDataContext =>
				_binding.Source == null
				&& _binding.ElementName == null
				&& _binding.RelativeSource == null;

			internal BindingRecord(
				FrameworkElement target,
				DependencyProperty dp,
				Binding binding,
				BindingExpression expr)
			{
				_targetRef = new WeakReference<FrameworkElement>(target);
				_dp = dp;
				_binding = binding;
				_expr = expr;
			}

			internal void Activate()
			{
				if (!_targetRef.TryGetTarget(out var target)) return;
				DetachInpc();

				var mode = EffectiveMode();
				if (mode == BindingMode.OneWayToSource)
				{
					SubscribeTargetChanges(target);
					WriteBack(target);
					return;
				}

				var source = ResolveSource(target);

				bool canResolve = source != null
					|| (!UsesDataContext)
					|| target.DataContext != null;

				if (!canResolve)
				{
					// Source unresolvable — apply FallbackValue.
					ApplyFallback(target);
				}
				else
				{
					PushToTarget(target, source);
				}

				if (mode == BindingMode.OneTime) return;

				if (source is INotifyPropertyChanged inpc)
				{
					_inpcSource = inpc;
					_inpcSource.PropertyChanged += OnSourcePropertyChanged;
					_watchedProperty = string.IsNullOrEmpty(_binding.Path)
						? null
						: _binding.Path!.Split('.')[0];
				}

				if (mode == BindingMode.TwoWay
					&& _binding.UpdateSourceTrigger != UpdateSourceTrigger.Explicit)
					SubscribeTargetChanges(target);
			}

			internal void Deactivate()
			{
				DetachInpc();
				UnsubscribeTargetChanges();
			}

			BindingMode EffectiveMode() =>
				_binding.Mode == BindingMode.Default ? BindingMode.OneWay : _binding.Mode;

			internal object? GetResolvedSource(FrameworkElement target) => ResolveSource(target);

			object? ResolveSource(FrameworkElement target)
			{
				if (_binding.Source != null) return _binding.Source;
				if (_binding.ElementName != null)
					return target.FindName(_binding.ElementName) as FrameworkElement;
				if (_binding.RelativeSource != null)
					return _binding.RelativeSource.Resolve(target);
				return target.DataContext;
			}

			void ApplyFallback(FrameworkElement target)
			{
				var fb = _binding.FallbackValue;
				if (!ReferenceEquals(fb, DependencyProperty.UnsetValue))
					target.SetValue(_dp, fb);
			}

			internal void PushToTargetPublic(FrameworkElement target) => PushToTarget(target, ResolveSource(target));

			void PushToTarget(FrameworkElement target, object? source)
			{
				_pushing = true;
				try
				{
					var rawValue = ResolvePath(source, _binding.Path);

					// FallbackValue when path resolves to null and no source.
					if (rawValue == null && source == null)
					{
						ApplyFallback(target);
						return;
					}

					object? value = rawValue;
					if (_binding.Converter != null)
						value = _binding.Converter.Convert(rawValue, _dp.PropertyType,
							_binding.ConverterParameter, CultureInfo.CurrentCulture);

					// TargetNullValue.
					if (value == null && _binding.TargetNullValue != null)
						value = _binding.TargetNullValue;

					// StringFormat.
					value = ApplyStringFormat(_binding.StringFormat, value);

					target.SetValue(_dp, value);
				}
				finally
				{
					_pushing = false;
				}
			}

			internal void WriteBackPublic(FrameworkElement target) => WriteBack(target);

			void WriteBack(FrameworkElement target)
			{
				if (_pushing) return;
				// UpdateSourceTrigger.Explicit: only write back when explicitly called.
				// (PropertyChanged / Default / LostFocus → auto; Explicit → only via WriteBackPublic)
				_pushing = true;
				try
				{
					var source = ResolveSource(target);
					if (source == null) return;

					var rawTargetValue = target.GetValue(_dp);
					object? sourceValue = rawTargetValue;
					if (_binding.Converter != null)
						sourceValue = _binding.Converter.ConvertBack(rawTargetValue, _dp.PropertyType,
							_binding.ConverterParameter, CultureInfo.CurrentCulture);

					WriteBackPath(source, _binding.Path, sourceValue);
				}
				finally
				{
					_pushing = false;
				}
			}

			PropertyChangedCallback? _targetChangedCallback;

			void SubscribeTargetChanges(FrameworkElement target)
			{
				_targetChangedCallback = OnTargetPropertyChanged;
				TargetChangeTracker.Subscribe(target, _dp, _targetChangedCallback);
			}

			void UnsubscribeTargetChanges()
			{
				if (_targetChangedCallback == null) return;
				if (_targetRef.TryGetTarget(out var target))
					TargetChangeTracker.Unsubscribe(target, _dp, _targetChangedCallback);
				_targetChangedCallback = null;
			}

			void OnTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
			{
				if (!_targetRef.TryGetTarget(out var target)) return;
				// Explicit: don't auto write back — wait for UpdateSource().
				if (_binding.UpdateSourceTrigger == UpdateSourceTrigger.Explicit) return;
				WriteBack(target);
			}

			void DetachInpc()
			{
				if (_inpcSource != null)
				{
					_inpcSource.PropertyChanged -= OnSourcePropertyChanged;
					_inpcSource = null;
					_watchedProperty = null;
				}
			}

			void OnSourcePropertyChanged(object? sender, PropertyChangedEventArgs e)
			{
				if (!_targetRef.TryGetTarget(out var target)) return;
				if (_watchedProperty != null
					&& e.PropertyName != null
					&& e.PropertyName != _watchedProperty) return;
				PushToTarget(target, sender);
			}
		}
	}
}
