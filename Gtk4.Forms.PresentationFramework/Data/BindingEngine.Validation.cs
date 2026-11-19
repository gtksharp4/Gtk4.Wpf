using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;

namespace System.Windows.Data;

internal static partial class BindingEngine
{
	internal sealed partial class BindingRecord
	{
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

	}
}
