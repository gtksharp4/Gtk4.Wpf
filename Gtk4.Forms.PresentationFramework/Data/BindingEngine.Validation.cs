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
		                      || ExceptionValidationApplies
		                      || _notifyWatcher != null;

		/// <summary>
		/// Runs the rules registered for <paramref name="step"/>; failures are appended to
		/// <paramref name="errors"/>, stopping at the first failure as WPF does. Returns <c>true</c> when all pass.
		/// </summary>
		bool RunRules(ValidationStep step, object? value, bool targetUpdate, List<ValidationError> errors)
		{
			if (_binding.ValidationRules.Count == 0) return true;
			var culture = ResolveCulture(_binding.ConverterCulture);
			foreach (var rule in _binding.ValidationRules)
			{
				if (rule.ValidationStep != step) continue;
				if (targetUpdate && !rule.ValidatesOnTargetUpdated) continue;
				ValidationResult result;
				try { result = rule.Validate(value, culture, _expr); }
				catch (Exception ex)
				{
					errors.Add(new ValidationError(rule, ex.Message, ex));
					return false;
				}
				if (result.IsValid) continue;
				errors.Add(new ValidationError(rule, result.ErrorContent));
				return false;
			}
			return true;
		}

		/// <summary>IDataErrorInfo of the leaf owner (property error, or entity error for an empty path).</summary>
		bool CollectDataErrors(object? source, List<ValidationError> errors)
		{
			if (!_binding.ValidatesOnDataErrors) return true;
			if (!TryGetLeaf(source, _binding.PathString, out var owner, out var prop)) return true;
			if (!(owner is IDataErrorInfo dei)) return true;
			var msg = prop == null ? dei.Error : dei[prop];
			if (string.IsNullOrEmpty(msg)) return true;
			errors.Add(new ValidationError(DataErrorValidationRule.Instance, msg));
			return false;
		}

		/// <summary>Records step errors (empty clears) and publishes them with the INDEI errors.</summary>
		void SetStepErrors(DependencyObject target, List<ValidationError> errors)
		{
			_stepErrors = errors;
			Publish(target);
		}

		void Publish(DependencyObject target)
		{
			int count = _stepErrors.Count + _notifyErrors.Count;
			if (count == 0 && !_published && !HasValidation) return;
			var all = new List<ValidationError>(_stepErrors);
			all.AddRange(_notifyErrors);
			Validation.SetErrorsFor(target, this, all, _binding.NotifyOnValidationError);
			_published = count > 0;
		}

		/// <summary>Leaf owner and last path segment (<c>null</c> for an empty path).</summary>
		static bool TryGetLeaf(object? source, string? path, out object owner, out string? prop)
		{
			owner = null!;
			prop = null;
			if (source == null) return false;
			path = PropertyPath.ResolveOwner(source, path);
			if (string.IsNullOrEmpty(path)) { owner = source; return true; }
			SplitLeaf(path!, out var headPath, out var leaf);
			var head = headPath.Length > 0 ? ResolvePath(source, headPath) : source;
			if (head == null) return false;
			owner = (leaf != null ? LeafOwner(head, leaf) : head) ?? head;
			prop = leaf;
			return true;
		}
	}
}
