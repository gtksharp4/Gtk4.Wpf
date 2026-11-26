namespace System.Windows.Data;

public partial class BindingGroup
{
	/// <summary>Whether a rule failure raises <c>Validation.Error</c>. Stored only; default false.</summary>
	public bool NotifyOnValidationError { get; set; }

	/// <summary>Whether INotifyDataErrorInfo errors are validated. Stored only; default true.</summary>
	public bool ValidatesOnNotifyDataError { get; set; } = true;

	/// <summary>Whether <see cref="CancelEdit"/> may restore values. Stored only; default false.</summary>
	public bool CanRestoreValues { get; set; }

	/// <summary>Whether proposed values are shared between bindings. Stored only; default false.</summary>
	public bool SharesProposedValues { get; set; }

	/// <summary>A group with the same name, flags and rules (the rule instances are shared).</summary>
	internal BindingGroup CloneForItem()
	{
		var copy = new BindingGroup
		{
			Name = Name,
			NotifyOnValidationError = NotifyOnValidationError,
			ValidatesOnNotifyDataError = ValidatesOnNotifyDataError,
			CanRestoreValues = CanRestoreValues,
			SharesProposedValues = SharesProposedValues,
		};
		foreach (var rule in ValidationRules) copy.ValidationRules.Add(rule);
		return copy;
	}
}
