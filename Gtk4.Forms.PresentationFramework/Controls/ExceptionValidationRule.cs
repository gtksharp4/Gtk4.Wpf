using System.Globalization;

namespace System.Windows.Controls;

/// <summary>
/// Represents a rule that checks for exceptions thrown during the update of the binding source value.
/// </summary>
public sealed class ExceptionValidationRule : ValidationRule
{
	/// <summary>Singleton instance.</summary>
	public static readonly ExceptionValidationRule Instance = new ExceptionValidationRule();

	/// <summary>Creates a rule that runs on <see cref="ValidationStep.RawProposedValue"/>.</summary>
	public ExceptionValidationRule()
		: base(ValidationStep.RawProposedValue, false)
	{
	}

	/// <summary>Always returns valid; exceptions are caught by the binding engine directly.</summary>
	public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
		=> ValidationResult.ValidResult;
}