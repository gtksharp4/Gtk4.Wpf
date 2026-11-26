namespace System.Windows.Controls;

/// <summary>Specifies when a <see cref="ValidationRule"/> runs during the source-update pipeline.</summary>
public enum ValidationStep
{
	/// <summary>Before any conversion, on the raw target value.</summary>
	RawProposedValue = 0,
	/// <summary>After conversion, on the value about to be written to the source.</summary>
	ConvertedProposedValue = 1,
	/// <summary>After the source was written; the rule receives the binding expression.</summary>
	UpdatedValue = 2,
	/// <summary>
	/// After the value is committed. Without a BindingGroup/IEditableObject commit cycle these run
	/// immediately after <see cref="UpdatedValue"/> rules.
	/// </summary>
	CommittedValue = 3,
}
