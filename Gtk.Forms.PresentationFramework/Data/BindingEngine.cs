using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Windows.Data
{
	/// <summary>
	/// Phase-1 binding engine: OneWay, TwoWay, OneTime, OneWayToSource.
	/// <para>
	/// Sets a binding on a target DP, resolves the source (explicit
	/// <see cref="Binding.Source"/>, <see cref="Binding.ElementName"/>, or the element's
	/// inherited <c>DataContext</c>), reads the value, pushes it into the target DP,
	/// and subscribes to <see cref="INotifyPropertyChanged"/> for future updates.
	/// </para>
	/// <para>
	/// TwoWay: also subscribes to target DP changes and writes back through the path
	/// using <see cref="IValueConverter.ConvertBack"/> when a converter is present.
	/// A reentrancy guard prevents feedback loops.
	/// </para>
	/// </summary>
	internal static class BindingEngine
	{
		// Registered bindings per target element (ConditionalWeakTable lets the entry
		// be collected with the element).
		static readonly ConditionalWeakTable<FrameworkElement, List<BindingRecord>> _bindings =
			new ConditionalWeakTable<FrameworkElement, List<BindingRecord>>();

		/// <summary>
		/// Registers <paramref name="binding"/> on <paramref name="dp"/> of
		/// <paramref name="target"/> and applies it immediately.
		/// </summary>
		internal static void SetBinding(FrameworkElement target, DependencyProperty dp, BindingBase bindingBase)
		{
			var list = _bindings.GetOrCreateValue(target);

			// Deactivate and remove any existing binding on the same DP.
			int idx = list.FindIndex(r => r.TargetProperty == dp);
			if (idx >= 0)
			{
				list[idx].Deactivate();
				list.RemoveAt(idx);
			}

			if (bindingBase is Binding binding)
			{
				var record = new BindingRecord(target, dp, binding);
				list.Add(record);
				record.Activate();
			}
		}

		/// <summary>
		/// Re-evaluates all bindings whose source is the inherited DataContext.
		/// Called when DataContext changes.
		/// </summary>
		internal static void RefreshBindings(FrameworkElement target)
		{
			if (!_bindings.TryGetValue(target, out var list)) return;
			foreach (var record in list)
			{
				// Skip records with an explicit source — they don't depend on DataContext.
				if (!record.UsesDataContext) continue;
				record.Activate();
			}
		}

		// ── Path resolution ──────────────────────────────────────────────────────

		/// <summary>
		/// Resolves a dot-separated path on <paramref name="source"/>.
		/// Returns <c>null</c> when any step is null or the property does not exist.
		/// </summary>
		internal static object? ResolvePath(object? source, string? path)
		{
			if (source == null) return null;
			if (string.IsNullOrEmpty(path)) return source;

			var current = source;
			foreach (var segment in path.Split('.'))
			{
				if (current == null) return null;
				var prop = current.GetType().GetProperty(
					segment,
					BindingFlags.Public | BindingFlags.Instance);
				if (prop == null) return null;
				current = prop.GetValue(current);
			}
			return current;
		}

		/// <summary>
		/// Writes <paramref name="value"/> back through the dot-separated path on
		/// <paramref name="source"/>, setting the final segment's property.
		/// Returns <c>true</c> on success.
		/// </summary>
		internal static bool WriteBackPath(object? source, string? path, object? value)
		{
			if (source == null) return false;
			if (string.IsNullOrEmpty(path))
			{
				// No path — the source itself is the target; we can't replace the reference.
				return false;
			}

			var current = source;
			var segments = path!.Split('.');
			// Walk all but the last segment.
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
				segments[segments.Length - 1],
				BindingFlags.Public | BindingFlags.Instance);
			if (lastProp == null || !lastProp.CanWrite) return false;

			lastProp.SetValue(current, value);
			return true;
		}

		// ── Inner record ────────────────────────────────────────────────────────

		sealed class BindingRecord
		{
			readonly WeakReference<FrameworkElement> _targetRef;
			readonly DependencyProperty _dp;
			readonly Binding _binding;

			// The INotifyPropertyChanged source we are currently subscribed to for forward flow.
			INotifyPropertyChanged? _inpcSource;

			// First segment of the path (what we listen on INPC for).
			string? _watchedProperty;

			// Reentrancy guard: prevents a TwoWay push from triggering a write-back loop.
			bool _pushing;

			internal DependencyProperty TargetProperty => _dp;

			/// <summary>
			/// True when the source is the element's DataContext (not an explicit Source or ElementName).
			/// </summary>
			internal bool UsesDataContext =>
				_binding.Source == null && _binding.ElementName == null;

			internal BindingRecord(FrameworkElement target, DependencyProperty dp, Binding binding)
			{
				_targetRef = new WeakReference<FrameworkElement>(target);
				_dp = dp;
				_binding = binding;
			}

			/// <summary>Resolves the source and pushes the current value.</summary>
			internal void Activate()
			{
				if (!_targetRef.TryGetTarget(out var target)) return;

				// Detach from previous INPC source.
				DetachInpc();

				var mode = EffectiveMode();
				if (mode == BindingMode.OneWayToSource)
				{
					// OneWayToSource: subscribe to target changes only, then do an initial push.
					SubscribeTargetChanges(target);
					WriteBack(target);
					return;
				}

				var source = ResolveSource(target);

				if (mode != BindingMode.OneWayToSource)
					PushToTarget(target, source);

				if (mode == BindingMode.OneTime) return; // no subscriptions

				// OneWay / TwoWay: subscribe to source INPC.
				if (source is INotifyPropertyChanged inpc)
				{
					_inpcSource = inpc;
					_inpcSource.PropertyChanged += OnSourcePropertyChanged;
					_watchedProperty = string.IsNullOrEmpty(_binding.Path)
						? null
						: _binding.Path!.Split('.')[0];
				}

				// TwoWay: also subscribe to target DP changes.
				if (mode == BindingMode.TwoWay)
					SubscribeTargetChanges(target);
			}

			/// <summary>Removes all subscriptions. Called when a binding is replaced.</summary>
			internal void Deactivate()
			{
				DetachInpc();
				UnsubscribeTargetChanges();
			}

			BindingMode EffectiveMode()
			{
				return _binding.Mode == BindingMode.Default ? BindingMode.OneWay : _binding.Mode;
			}

			object? ResolveSource(FrameworkElement target)
			{
				if (_binding.Source != null) return _binding.Source;
				if (_binding.ElementName != null)
					return target.FindName(_binding.ElementName) as FrameworkElement;
				return target.DataContext;
			}

			void PushToTarget(FrameworkElement target, object? source)
			{
				_pushing = true;
				try
				{
					var rawValue = ResolvePath(source, _binding.Path);
					object? value = rawValue;
					if (_binding.Converter != null)
						value = _binding.Converter.Convert(rawValue, _dp.PropertyType,
							_binding.ConverterParameter, CultureInfo.CurrentCulture);
					target.SetValue(_dp, value);
				}
				finally
				{
					_pushing = false;
				}
			}

			void WriteBack(FrameworkElement target)
			{
				if (_pushing) return; // reentrancy guard
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

			// ── Target DP change subscription ────────────────────────────────────

			// Keep the handler as a delegate so we can detach by reference.
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
				WriteBack(target);
			}

			// ── INPC ─────────────────────────────────────────────────────────────

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
				if (_watchedProperty != null &&
					e.PropertyName != null &&
					e.PropertyName != _watchedProperty) return;
				PushToTarget(target, sender);
			}
		}
	}
}
