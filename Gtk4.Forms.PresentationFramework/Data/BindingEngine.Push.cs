using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Data;

internal static partial class BindingEngine
{
	internal sealed partial class BindingRecord
	{
		void ApplyFallback(FrameworkElement target)
			=> SetFallbackOrDefault(target, _dp, _binding.FallbackValue);

		void ApplyTargetNullValue(FrameworkElement target)
		{
			var tnv = _binding.TargetNullValue;
			if (tnv == null) return;
			if (TryCoerceValid(_dp, tnv, out var converted))
				target.SetValue(_dp, converted);
			else
				SetFallbackOrDefault(target, _dp, _binding.FallbackValue);
		}

		internal void PushToTargetPublic(FrameworkElement target) => PushToTarget(target, ResolveSource(target));

		void PushToTarget(FrameworkElement target, object? source)
		{
			PushToTargetCore(target, source);
			RunTargetUpdatedValidation(target, source);
		}

		void PushToTargetCore(FrameworkElement target, object? source)
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
				{
					value = _binding.Converter.Convert(rawValue, _dp.PropertyType,
						_binding.ConverterParameter, ResolveCulture(_binding.ConverterCulture));
					// WPF: DoNothing aborts the transfer; UnsetValue means fallback/default.
					if (ReferenceEquals(value, Binding.DoNothing)) return;
					if (ReferenceEquals(value, DependencyProperty.UnsetValue))
					{
						ApplyFallback(target);
						return;
					}
				}

				// R5: TargetNullValue applied when resolved+converted value is null.
				if (value == null && _binding.TargetNullValue != null)
				{
					ApplyTargetNullValue(target);
					return;
				}

				// StringFormat.
				value = ApplyStringFormat(_binding.StringFormat, value, ResolveCulture(_binding.ConverterCulture));

				if (_binding.Converter == null)
				{
					try { value = CoerceToDpType(_dp, value); }
					catch (Exception ex) when (!IsCritical(ex))
					{
						ApplyFallback(target); // WPF falls back instead of throwing
						return;
					}
				}
				SetTargetSafe(target, _dp, value, _binding.FallbackValue);
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

		void DetachInpc()
		{
			DetachDpSource(); // BindingEngine.SourceDp.cs
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
