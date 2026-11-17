namespace System.Windows.Data;

/// <summary>
/// Describes the timing of binding source updates in a TwoWay or OneWayToSource binding.
/// </summary>
public enum UpdateSourceTrigger
{
	/// <summary>Default for the target property (usually PropertyChanged).</summary>
	Default,
	/// <summary>Update the source whenever the target property value changes.</summary>
	PropertyChanged,
	/// <summary>Update the source when the target element loses focus.</summary>
	LostFocus,
	/// <summary>Update the source only when <see cref="BindingExpression.UpdateSource"/> is called.</summary>
	Explicit,
}