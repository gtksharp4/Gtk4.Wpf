using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace System.Windows.Data;

internal static partial class BindingEngine
{
	internal sealed partial class BindingRecord
	{
		List<ValidationError> _stepErrors = new List<ValidationError>();
		bool _published;

		bool HasValidation => _binding.ValidationRules.Count > 0
		                      || _binding.ValidatesOnDataErrors
		                      || _binding.ValidatesOnExceptions
		                      || _notifyWatcher != null;

		/// <summary>
		/// Runs the rules registered for <paramref name="step"/>; failures are appended to
		/// <paramref name="errors"/>. Returns <c>true</c> when all pass.
		/// </summary>
		bool RunRules(ValidationStep step, object? value, bool targetUpdate, List<ValidationError> errors)
		{
			if (_binding.ValidationRules.Count == 0) return true;
			var culture = ResolveCulture(_binding.ConverterCulture);
			bool ok = true;
			foreach (var rule in _binding.ValidationRules)
			{
				if (rule.ValidationStep != step) continue;
				if (targetUpdate && !rule.ValidatesOnTargetUpdated) continue;
				ValidationResult result;
				try { result = rule.Validate(value, culture, _expr); }
				catch (Exception ex)
				{
					errors.Add(new ValidationError(rule, ex.Message, ex));
					ok = false;
					continue;
				}
				if (result.IsValid) continue;
				errors.Add(new ValidationError(rule, result.ErrorContent));
				ok = false;
			}
			return ok;
		}

		/// <summary>IDataErrorInfo of the leaf owner (property error, or entity error for an empty path).</summary>
		bool CollectDataErrors(object? source, List<ValidationError> errors)
		{
			if (!_binding.ValidatesOnDataErrors) return true;
			if (!TryGetLeaf(source, _binding.Path, out var owner, out var prop)) return true;
			if (!(owner is IDataErrorInfo dei)) return true;
			var msg = prop == null ? dei.Error : dei[prop];
			if (string.IsNullOrEmpty(msg)) return true;
			errors.Add(new ValidationError(DataErrorValidationRule.Instance, msg));
			return false;
		}

		/// <summary>Records step errors (empty clears) and publishes them with the INDEI errors.</summary>
		void SetStepErrors(FrameworkElement target, List<ValidationError> errors)
		{
			_stepErrors = errors;
			Publish(target);
		}

		void Publish(FrameworkElement target)
		{
			int count = _stepErrors.Count + _notifyErrors.Count;
			if (count == 0 && !_published && !HasValidation) return;
			var all = new List<ValidationError>(_stepErrors);
			all.AddRange(_notifyErrors);
			Validation.SetErrors(target, all, _binding.NotifyOnValidationError);
			_published = count > 0;
		}

		/// <summary>Leaf owner and last path segment (<c>null</c> for an empty path).</summary>
		static bool TryGetLeaf(object? source, string? path, out object owner, out string? prop)
		{
			owner = null!;
			prop = null;
			if (source == null) return false;
			if (string.IsNullOrEmpty(path)) { owner = source; return true; }
			var segments = path!.Split('.');
			var head = segments.Length > 1
				? ResolvePath(source, string.Join(".", segments, 0, segments.Length - 1))
				: source;
			if (head == null) return false;
			owner = head;
			prop = segments[segments.Length - 1];
			return true;
		}
	}
}
