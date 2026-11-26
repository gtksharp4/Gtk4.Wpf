using System.Globalization;

namespace System.Windows.Controls;

/// <summary>
/// Represents a rule that checks for errors raised by <see cref="System.ComponentModel.INotifyDataErrorInfo"/>.
/// </summary>
public sealed class NotifyDataErrorValidationRule : ValidationRule
{
	internal static readonly NotifyDataErrorValidationRule Instance = new NotifyDataErrorValidationRule();

	/// <summary>Creates a rule that runs on <see cref="ValidationStep.UpdatedValue"/> and on target updates.</summary>
	public NotifyDataErrorValidationRule()
		: base(ValidationStep.UpdatedValue, true)
	{
	}

	/// <summary>Always valid; the binding engine queries the source and tracks ErrorsChanged itself.</summary>
	public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
		=> ValidationResult.ValidResult;
}
