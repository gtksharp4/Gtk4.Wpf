namespace System.Windows;

public partial class DependencyObject
{
	// Called before an explicit SetValue/ClearValue touches the local slot, so a subclass can detach a
	// live link that owns that slot.
	internal virtual void OnExplicitLocalChange(DependencyProperty dp) { }
}
