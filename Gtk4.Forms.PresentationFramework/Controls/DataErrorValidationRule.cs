using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Controls;

/// <summary>
/// Represents a rule that checks for errors raised by the <see cref="IDataErrorInfo"/> interface.
/// </summary>
public sealed class DataErrorValidationRule : ValidationRule
{
	/// <summary>Singleton instance.</summary>
	public static readonly DataErrorValidationRule Instance = new DataErrorValidationRule();

	/// <summary>Creates a rule that runs on <see cref="ValidationStep.UpdatedValue"/> and on target updates.</summary>
	public DataErrorValidationRule()
		: base(ValidationStep.UpdatedValue, true)
	{
	}

	/// <summary>
	/// Marker: <see cref="Data.BindingEngine"/> calls <c>CollectDataErrors</c> directly on the
	/// source when <c>ValidatesOnDataErrors</c> is set; this <c>Validate</c> body is never invoked
	/// by the engine. It is kept as a usable fallback for callers that run validation rules manually.
	/// </summary>
	public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
	{
		// Not called by the binding engine — see BindingEngine.CollectDataErrors.
		if (value is IDataErrorInfo dataError)
		{
			var msg = dataError.Error;
			if (!string.IsNullOrEmpty(msg))
				return new ValidationResult(false, msg);
		}
		return ValidationResult.ValidResult;
	}
}