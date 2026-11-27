using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Data;

internal static partial class BindingEngine
{
	internal sealed partial class BindingRecord
	{
		void ApplyFallback(DependencyObject target)
			=> SetFallbackOrDefault(target, _dp, _binding.FallbackValue);

		void ApplyTargetNullValue(DependencyObject target)
		{
			var tnv = _binding.TargetNullValue;
			if (tnv == null) return;
			if (TryCoerceValid(_dp, tnv, out var converted))
				target.SetValue(_dp, converted);
			else
				SetFallbackOrDefault(target, _dp, _binding.FallbackValue);
		}

		internal void PushToTargetPublic(DependencyObject target) => PushToTarget(target, ResolveSource(target));

		void PushToTarget(DependencyObject target, object? source)
		{
			bool transferred = PushToTargetCore(target, source);
			RunTargetUpdatedValidation(target, source, _lastRaw);
			if (transferred) RaiseDataTransfer(target, Binding.TargetUpdatedEvent, _binding.NotifyOnTargetUpdated);
			RefreshCurrentViews(source);
		}

		object? _lastRaw;

		/// <summary>Transfers the source value; <see langword="false"/> when the transfer was aborted.</summary>
		bool PushToTargetCore(DependencyObject target, object? source)
		{
			_pushing = true;
			try
			{
				// R5: FallbackValue applies when the path is unresolvable:
				// either source is null (no DataContext, no explicit source), OR
				// source is non-null but the path itself doesn't resolve.
				var rawValue = ResolvePath(source, _binding.PathString);
				_lastRaw = rawValue;

				bool pathUnresolvable = rawValue == null
				                        && (source == null || PathIsUnresolvable(source));

				if (pathUnresolvable)
				{
					ApplyFallback(target);
					return true;
				}

				object? value = rawValue;
				// Converter runs before TargetNullValue (R5).
				if (_binding.Converter != null)
				{
					value = _binding.Converter.Convert(rawValue, _dp.PropertyType,
						_binding.ConverterParameter, ResolveCulture(_binding.ConverterCulture));
					// WPF: DoNothing aborts the transfer; UnsetValue means fallback/default.
					if (ReferenceEquals(value, Binding.DoNothing)) return false;
					if (ReferenceEquals(value, DependencyProperty.UnsetValue))
					{
						ApplyFallback(target);
						return true;
					}
				}

				// R5: TargetNullValue applied when resolved+converted value is null.
				if (value == null && _binding.TargetNullValue != null)
				{
					ApplyTargetNullValue(target);
					return true;
				}

				// StringFormat.
				value = ApplyStringFormat(_binding.StringFormat, value, ResolveCulture(_binding.ConverterCulture));

				if (_binding.Converter == null)
				{
					try { value = CoerceToDpType(_dp, value); }
					catch (Exception ex) when (!IsCritical(ex))
					{
						ApplyFallback(target); // WPF falls back instead of throwing
						return true;
					}
				}
				SetTargetSafe(target, _dp, value, _binding.FallbackValue);
				return true;
			}
			finally
			{
				_pushing = false;
			}
		}

		void RaiseDataTransfer(DependencyObject target, RoutedEvent routedEvent, bool notify)
		{
			if (notify) target.RaiseEvent(new DataTransferEventArgs(routedEvent, target, _dp) { Source = target });
		}

		/// <summary>
		/// Returns <c>true</c> when <paramref name="source"/> is non-null but the binding
		/// path does not resolve on it (the path segment is missing or null-traversal
		/// short-circuits). This distinguishes a legitimately null value from a bad path.
		/// </summary>
		bool PathIsUnresolvable(object source)
		{
			return !TryWalk(source, _binding.PathString, null, out _);
		}

		void DetachInpc()
		{
			if (_watchedProvider != null)
			{
				_watchedProvider.DataChanged -= OnProviderDataChanged;
				_watchedProvider = null;
			}
			DetachDpSource(); // BindingEngine.SourceDp.cs
			DetachViews();
			_watchViews = false;
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
