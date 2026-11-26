using System.Globalization;
using System.Windows.Data;

namespace System.Windows.Controls;

/// <summary>
/// Provides a way to create a custom rule in order to check the validity of user input.
/// </summary>
public abstract class ValidationRule
{
	/// <summary>Initialises a rule that runs on <see cref="ValidationStep.RawProposedValue"/>.</summary>
	protected ValidationRule()
		: this(ValidationStep.RawProposedValue, false)
	{
	}

	/// <summary>Initialises a rule with the given step and target-updated behaviour.</summary>
	protected ValidationRule(ValidationStep validationStep, bool validatesOnTargetUpdated)
	{
		ValidationStep = validationStep;
		ValidatesOnTargetUpdated = validatesOnTargetUpdated;
	}

	/// <summary>The pipeline step at which the rule runs. Default is <see cref="ValidationStep.RawProposedValue"/>.</summary>
	public ValidationStep ValidationStep { get; set; }

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

	/// <summary>
	/// Validates for a binding expression. For the Updated/Committed steps the owner replaces the value.
	/// </summary>
	public virtual ValidationResult Validate(object? value, CultureInfo cultureInfo, BindingExpressionBase owner)
	{
		if (ValidationStep == ValidationStep.UpdatedValue || ValidationStep == ValidationStep.CommittedValue)
			value = owner;
		return Validate(value, cultureInfo);
	}

	/// <summary>
	/// Validates for a binding group. For the Updated/Committed steps the owner replaces the value.
	/// </summary>
	public virtual ValidationResult Validate(object? value, CultureInfo cultureInfo, BindingGroup owner)
	{
		if (ValidationStep == ValidationStep.UpdatedValue || ValidationStep == ValidationStep.CommittedValue)
			value = owner;
		return Validate(value, cultureInfo);
	}
}
