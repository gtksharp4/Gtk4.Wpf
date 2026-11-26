namespace System.Windows.Controls;

/// <summary>
/// Selects a <see cref="Style"/> for a data item in an items control.
/// Override <see cref="SelectStyle"/> to implement custom selection logic.
/// </summary>
public class StyleSelector
{
	/// <summary>
	/// Returns a <see cref="Style"/> for the given <paramref name="item"/>
	/// displayed in <paramref name="container"/>.
	/// Returns <c>null</c> (falls back to <see cref="ItemsControl.ItemContainerStyle"/>).
	/// </summary>
	public virtual Style? SelectStyle(object? item, DependencyObject? container)
		=> null;
}
