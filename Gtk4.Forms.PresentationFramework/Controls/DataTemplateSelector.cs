namespace System.Windows.Controls;

/// <summary>
/// Selects a <see cref="DataTemplate"/> for a data item in an items control.
/// Override <see cref="SelectTemplate"/> to implement custom selection logic.
/// </summary>
public class DataTemplateSelector
{
	/// <summary>
	/// Returns a <see cref="DataTemplate"/> for the given <paramref name="item"/>
	/// displayed in <paramref name="container"/>.
	/// Returns <c>null</c> (falls back to <see cref="ItemsControl.ItemTemplate"/>).
	/// </summary>
	public virtual DataTemplate? SelectTemplate(object? item, DependencyObject? container)
		=> null;
}
