using System.Collections.Generic;
using System.Windows.Controls;

namespace System.Windows.Data;

internal static partial class BindingEngine
{
	internal sealed partial class BindingRecord
	{
		/// <summary>Whether a source-update exception becomes a validation error.</summary>
		bool ExceptionValidationApplies
		{
			get
			{
				if (_binding.ValidatesOnExceptions) return true;
				foreach (var rule in _binding.ValidationRules)
					if (rule is ExceptionValidationRule) return true;
				return false;
			}
		}

		/// <summary>The error for <paramref name="ex"/> after the exception filter, or null for none.</summary>
		ValidationError? ErrorForException(Exception ex)
		{
			var filter = _binding.UpdateSourceExceptionFilter;
			if (filter == null) return new ValidationError(ExceptionValidationRule.Instance, ex.Message, ex);
			var result = filter(_expr, ex);
			if (result == null) return null;
			return result as ValidationError ?? new ValidationError(ExceptionValidationRule.Instance, result, ex);
		}

		void ReportError(DependencyObject target, Exception ex)
		{
			var err = ErrorForException(ex);
			SetStepErrors(target, err == null ? new List<ValidationError>() : new List<ValidationError> { err });
		}
	}
}
