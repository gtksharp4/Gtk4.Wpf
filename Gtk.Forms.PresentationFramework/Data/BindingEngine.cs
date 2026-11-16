using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

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

			// Clear any existing simple-binding record for this DP.
			int idx = list.FindIndex(r => r.TargetProperty == dp);
			if (idx >= 0)
			{
				list[idx].Deactivate();
				list.RemoveAt(idx);
			}

			// Also clear any existing multi-binding record for this DP (R2: cross-engine leak).
			MultiBindingEngine.ClearForDp(target, dp);

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

		/// <summary>
		/// Re-activates all <see cref="Binding"/>s whose
		/// <c>RelativeSource.Mode == RelativeSourceMode.TemplatedParent</c> on every element
		/// in the subtree rooted at <paramref name="root"/> (including <paramref name="root"/>
		/// itself). Called by <see cref="Controls.Control.OnTemplateChanged"/> immediately
		/// after <c>StampTemplatedParent</c> stamps the new <c>TemplatedParent</c> value so
		/// that the bindings can resolve the source that was null at activation time.
		/// </summary>
		internal static void RefreshTemplatedParentBindings(UIElement root)
		{
			RefreshTemplatedParentOnElement(root);
			foreach (var child in root.VisualChildren)
				RefreshTemplatedParentBindings(child);
		}

		static void RefreshTemplatedParentOnElement(UIElement element)
		{
			if (element is not FrameworkElement fe) return;
			if (!_bindings.TryGetValue(fe, out var list)) return;
			foreach (var record in list)
			{
				if (record.UsesTemplatedParent)
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

			internal bool UsesTemplatedParent =>
				_binding.RelativeSource?.Mode == RelativeSourceMode.TemplatedParent;

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
				// PushToTarget handles FallbackValue / TargetNullValue internally (R5).
				PushToTarget(target, source);

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
				if (ReferenceEquals(fb, DependencyProperty.UnsetValue)) return;
				// R4: convert string FallbackValue to target DP type when needed.
				fb = CoerceToTargetType(fb);
				target.SetValue(_dp, fb);
			}

			void ApplyTargetNullValue(FrameworkElement target)
			{
				var tnv = _binding.TargetNullValue;
				if (tnv == null) return;
				// R4: convert string TargetNullValue to target DP type when needed.
				tnv = CoerceToTargetType(tnv);
				target.SetValue(_dp, tnv);
			}

			/// <summary>
			/// If <paramref name="value"/> is a string but the target DP type is not string,
			/// converts it via <see cref="System.Windows.Markup.XamlValueConverter"/> (R4).
			/// </summary>
			object? CoerceToTargetType(object? value)
			{
				if (value is string s && _dp.PropertyType != typeof(string))
					return System.Windows.Markup.XamlValueConverter.Convert(null!, _dp, s);
				return value;
			}

			internal void PushToTargetPublic(FrameworkElement target) => PushToTarget(target, ResolveSource(target));

			void PushToTarget(FrameworkElement target, object? source)
			{
				_pushing = true;
				try
				{
					// R5: FallbackValue applies when the path is unresolvable:
					// either source is null (no DataContext, no explicit source), OR
					// source is non-null but the path itself doesn't resolve.
					var rawValue = ResolvePath(source, _binding.Path);

					bool pathUnresolvable = rawValue == null
						&& (source == null || PathIsUnresolvable(source));

					if (pathUnresolvable)
					{
						ApplyFallback(target);
						return;
					}

					object? value = rawValue;
					// Converter runs before TargetNullValue (R5).
					if (_binding.Converter != null)
						value = _binding.Converter.Convert(rawValue, _dp.PropertyType,
							_binding.ConverterParameter, CultureInfo.CurrentCulture);

					// R5: TargetNullValue applied when resolved+converted value is null.
					if (value == null && _binding.TargetNullValue != null)
					{
						ApplyTargetNullValue(target);
						return;
					}

					// StringFormat.
					value = ApplyStringFormat(_binding.StringFormat, value);

					target.SetValue(_dp, value);
				}
				finally
				{
					_pushing = false;
				}
			}

			/// <summary>
			/// Returns <c>true</c> when <paramref name="source"/> is non-null but the binding
			/// path does not resolve on it (the path segment is missing or null-traversal
			/// short-circuits). This distinguishes a legitimately null value from a bad path.
			/// </summary>
			bool PathIsUnresolvable(object source)
			{
				if (string.IsNullOrEmpty(_binding.Path)) return false;
				// Walk the path; if any segment's property is not found the path is unresolvable.
				var current = (object?)source;
				foreach (var segment in _binding.Path!.Split('.'))
				{
					if (current == null) return true;
					var prop = current.GetType().GetProperty(
						segment, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
					if (prop == null) return true;
					current = prop.GetValue(current);
				}
				return false;
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

					// ── Validation (pre-write: explicit rules only) ──────────────
					if (!RunPreWriteValidation(target, sourceValue)) return;

					// ── Write to source ───────────────────────────────────────────
					if (_binding.ValidatesOnExceptions)
					{
						try { WriteBackPath(source, _binding.Path, sourceValue); }
						catch (Exception ex)
						{
							// Unwrap TargetInvocationException from reflection invoke.
							var inner = (ex is System.Reflection.TargetInvocationException tie
								&& tie.InnerException != null) ? tie.InnerException : ex;
							var err = new ValidationError(ExceptionValidationRule.Instance, inner.Message, inner);
							Validation.SetErrors(target, new[] { err }, _binding.NotifyOnValidationError);
							return;
						}
					}
					else
					{
						WriteBackPath(source, _binding.Path, sourceValue);
					}

					// ── Post-write: data error validation ─────────────────────────
					if (_binding.ValidatesOnDataErrors)
						RunPostWriteDataErrorValidation(target, source, _binding.Path);
					else
						Validation.ClearErrors(target, _binding.NotifyOnValidationError);
				}
				finally
				{
					_pushing = false;
				}
			}

			/// <summary>
			/// Runs explicit <see cref="ValidationRule"/>s before writing the value.
			/// Returns <c>true</c> when all rules pass (write may proceed).
			/// </summary>
			bool RunPreWriteValidation(FrameworkElement target, object? value)
			{
				if (_binding.ValidationRules.Count == 0) return true;

				var errors = new List<ValidationError>();
				foreach (var rule in _binding.ValidationRules)
				{
					ValidationResult result;
					try { result = rule.Validate(value, CultureInfo.CurrentCulture); }
					catch (Exception ex) { errors.Add(new ValidationError(rule, ex.Message, ex)); continue; }
					if (!result.IsValid) errors.Add(new ValidationError(rule, result.ErrorContent));
				}

				if (errors.Count > 0)
				{
					Validation.SetErrors(target, errors, _binding.NotifyOnValidationError);
					return false;
				}

				Validation.ClearErrors(target, _binding.NotifyOnValidationError);
				return true;
			}

			/// <summary>
			/// Checks <see cref="IDataErrorInfo"/> / <see cref="INotifyDataErrorInfo"/> on
			/// <paramref name="source"/> after the write has succeeded.
			/// </summary>
			void RunPostWriteDataErrorValidation(FrameworkElement target, object source, string? path)
			{
				var errors = new List<ValidationError>(CollectDataErrors(source, path));
				if (errors.Count > 0)
					Validation.SetErrors(target, errors, _binding.NotifyOnValidationError);
				else
					Validation.ClearErrors(target, _binding.NotifyOnValidationError);
			}

			static IEnumerable<ValidationError> CollectDataErrors(object source, string? path)
			{
				// Determine the property name (last path segment or empty for entity).
				string propName = string.Empty;
				if (!string.IsNullOrEmpty(path))
				{
					var parts = path!.Split('.');
					propName = parts[parts.Length - 1];
				}

				if (source is IDataErrorInfo dei)
				{
					var msg = string.IsNullOrEmpty(propName) ? dei.Error : dei[propName];
					if (!string.IsNullOrEmpty(msg))
						yield return new ValidationError(DataErrorValidationRule.Instance, msg);
				}

				if (source is INotifyDataErrorInfo ndei)
				{
					var errorEnum = ndei.GetErrors(string.IsNullOrEmpty(propName) ? null : propName);
					if (errorEnum != null)
					{
						foreach (var err in errorEnum)
						{
							yield return new ValidationError(DataErrorValidationRule.Instance,
								err?.ToString() ?? string.Empty);
						}
					}
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
