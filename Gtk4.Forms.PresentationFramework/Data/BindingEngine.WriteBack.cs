using System.Globalization;
using System.Reflection;
using System.Windows.Controls;

namespace System.Windows.Data;

internal static partial class BindingEngine
{
	internal sealed partial class BindingRecord
	{
		bool _hasWriteError;

		internal void WriteBackPublic(FrameworkElement target) => WriteBack(target);

		void WriteBack(FrameworkElement target)
		{
			if (_pushing) return;
			_dirty = false;
			_pushing = true;
			try
			{
				var source = ResolveSource(target);
				if (source == null) return;

				object? sourceValue = target.GetValue(_dp);
				if (_binding.Converter != null)
				{
					try
					{
						sourceValue = _binding.Converter.ConvertBack(sourceValue, _dp.PropertyType,
							_binding.ConverterParameter, CultureInfo.CurrentCulture);
					}
					catch (Exception ex) when (!IsCritical(ex))
					{
						// WPF skips the write; an error is recorded only when ValidatesOnExceptions is set.
						if (_binding.ValidatesOnExceptions) ReportError(target, ex);
						return;
					}
					if (ReferenceEquals(sourceValue, Binding.DoNothing)
					    || ReferenceEquals(sourceValue, DependencyProperty.UnsetValue)) return;
				}
				else if (TryResolveWriteTarget(source, _binding.Path, out _, out var prop)
				         && !TryConvertToSourceType(sourceValue, prop.PropertyType, out sourceValue, out var cex))
				{
					// WPF always reports a failed conversion as a validation error.
					ReportError(target, cex!);
					return;
				}

				// ── Validation (pre-write: explicit rules only) ──────────────
				if (!RunPreWriteValidation(target, sourceValue)) return;

				if (_binding.ValidatesOnExceptions)
				{
					try { WriteBackPath(source, _binding.Path, sourceValue); }
					catch (Exception ex) when (!IsCritical(ex))
					{
						ReportError(target, ex is TargetInvocationException tie && tie.InnerException != null
							? tie.InnerException : ex);
						return;
					}
				}
				else
					WriteBackPath(source, _binding.Path, sourceValue);

				// U4: skip validation work entirely when the binding has no validation rules.
				bool hasValidation = _binding.ValidationRules.Count > 0
				                     || _binding.ValidatesOnDataErrors
				                     || _binding.ValidatesOnExceptions;
				if (!hasValidation)
				{
					if (_hasWriteError) Validation.ClearErrors(target, _binding.NotifyOnValidationError);
					_hasWriteError = false;
					return;
				}

				_hasWriteError = false;
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

		void ReportError(FrameworkElement target, Exception ex)
		{
			_hasWriteError = true;
			var err = new ValidationError(ExceptionValidationRule.Instance, ex.Message, ex);
			Validation.SetErrors(target, new[] { err }, _binding.NotifyOnValidationError);
		}
	}
}
