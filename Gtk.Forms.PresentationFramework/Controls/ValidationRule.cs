using System.Globalization;

namespace System.Windows.Controls;

/// <summary>
/// Provides a way to create a custom rule in order to check the validity of user input.
/// </summary>
public abstract class ValidationRule
{
	/// <summary>
	/// When <c>true</c> the rule runs on each update from the source (not only from the target).
	/// Default is <c>false</c>.
	/// </summary>
	public bool ValidatesOnTargetUpdated { get; set; }

	/// <summary>Performs validation checks on a value.</summary>
	/// <param name="value">The value to check.</param>
	/// <param name="cultureInfo">The culture to use in this rule.</param>
	/// <returns>A <see cref="ValidationResult"/> object.</returns>
	public abstract ValidationResult Validate(object? value, CultureInfo cultureInfo);
}