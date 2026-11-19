namespace System.Windows.Data;

/// <summary>
/// Describes the timing of binding source updates in a TwoWay or OneWayToSource binding.
/// Lives in WindowsBase so <see cref="FrameworkPropertyMetadata"/> can carry a default trigger.
/// </summary>
public enum UpdateSourceTrigger
{
	/// <summary>Default for the target property (the metadata's default trigger, else PropertyChanged).</summary>
	Default,
	/// <summary>Update the source whenever the target property value changes.</summary>
	PropertyChanged,
	/// <summary>Update the source when the target element loses focus.</summary>
	LostFocus,
	/// <summary>Update the source only when <c>BindingExpression.UpdateSource</c> is called.</summary>
	Explicit,
}
