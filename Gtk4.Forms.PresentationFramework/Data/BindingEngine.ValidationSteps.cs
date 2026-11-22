using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Controls;

namespace System.Windows.Data;

internal static partial class BindingEngine
{
	internal sealed partial class BindingRecord
	{
		/// <summary>
		/// Source update pipeline: raw, convert, converted, write, updated, committed. The first failing
		/// step stops the rest. There is no BindingGroup/IEditableObject commit cycle, so committed rules
		/// run directly after the updated rules.
		/// </summary>
		void WriteBackCore(DependencyObject target, object source)
		{
			var culture = ResolveCulture(_binding.ConverterCulture);
			var errors = new List<ValidationError>();
			object? raw = target.GetValue(_dp);
			if (!RunRules(ValidationStep.RawProposedValue, raw, false, errors))
			{
				SetStepErrors(target, errors);
				return;
			}

			if (!TryConvertBack(target, source, raw, culture, out var sourceValue)) return;
			if (!RunRules(ValidationStep.ConvertedProposedValue, sourceValue, false, errors))
			{
				SetStepErrors(target, errors);
				return;
			}

			if (!WriteSource(target, source, sourceValue)) return;
			RaiseDataTransfer(target, Binding.SourceUpdatedEvent, _binding.NotifyOnSourceUpdated);

			AttachNotifyErrors(target, source, ResolvePath(source, _binding.PathString));
			RefreshNotifyErrors();
			bool ok = RunRules(ValidationStep.UpdatedValue, _expr, false, errors);
			if (ok) ok = CollectDataErrors(source, errors);
			if (ok) RunRules(ValidationStep.CommittedValue, _expr, false, errors);
			SetStepErrors(target, errors);
		}

		bool TryConvertBack(
			DependencyObject target, object source, object? raw, CultureInfo culture, out object? sourceValue)
		{
			sourceValue = raw;
			if (_binding.Converter != null)
			{
				try
				{
					sourceValue = _binding.Converter.ConvertBack(raw, _dp.PropertyType,
						_binding.ConverterParameter, culture);
				}
				catch (Exception ex) when (!IsCritical(ex))
				{
					// WPF skips the write; an error is recorded only when exception validation applies.
					if (ExceptionValidationApplies) ReportError(target, ex);
					return false;
				}
				return !ReferenceEquals(sourceValue, Binding.DoNothing)
				       && !ReferenceEquals(sourceValue, DependencyProperty.UnsetValue);
			}

			if (TryResolveWriteTarget(source, _binding.PathString, out _, out var prop)
			    && !TryConvertToSourceType(raw, prop.PropertyType, out sourceValue, out var cex, culture))
			{
				// WPF always reports a failed conversion as a validation error.
				ReportError(target, cex!);
				return false;
			}
			return true;
		}

		bool WriteSource(DependencyObject target, object source, object? sourceValue)
		{
			if (!ExceptionValidationApplies)
			{
				WriteBackPath(source, _binding.PathString, sourceValue);
				return true;
			}
			try { WriteBackPath(source, _binding.PathString, sourceValue); }
			catch (Exception ex) when (!IsCritical(ex))
			{
				ReportError(target, ex is TargetInvocationException tie && tie.InnerException != null
					? tie.InnerException : ex);
				return false;
			}
			return true;
		}

		/// <summary>Runs ValidatesOnTargetUpdated rules (and IDataErrorInfo) after a source-to-target transfer.</summary>
		void RunTargetUpdatedValidation(DependencyObject target, object? source, object? resolved)
		{
			AttachNotifyErrors(target, source, resolved);
			RefreshNotifyErrors();
			bool rules = _binding.ValidatesOnDataErrors;
			foreach (var r in _binding.ValidationRules)
				rules |= r.ValidatesOnTargetUpdated;
			if (!rules)
			{
				if (_notifyWatcher != null || _notifyErrors.Count > 0) Publish(target);
				return;
			}

			var errors = new List<ValidationError>();
			bool ok = RunRules(ValidationStep.RawProposedValue, target.GetValue(_dp), true, errors)
			          && RunRules(ValidationStep.ConvertedProposedValue,
				          resolved, true, errors);
			if (ok) ok = RunRules(ValidationStep.UpdatedValue, _expr, true, errors);
			if (ok) ok = CollectDataErrors(source, errors);
			if (ok) RunRules(ValidationStep.CommittedValue, _expr, true, errors);
			SetStepErrors(target, errors);
		}
	}
}
