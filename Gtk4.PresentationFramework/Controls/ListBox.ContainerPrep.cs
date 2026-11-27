namespace System.Windows.Controls;

public partial class ListBox
{
	// WPF ItemContainerGenerator: a container made for a data item gets the item as its DataContext, and its
	// style comes from the selector, ItemContainerStyle or an implicit ListBoxItem style, in that order.
	void PrepareContainer(ListBoxItem container, object? item)
	{
		container.LogicalParent = this;
		if (item != null && item is not UIElement)
			container.DataContext = item;

		var style = ItemContainerStyleSelector?.SelectStyle(item, container) ?? ItemContainerStyle;
		if (style == null && container.TryFindResource(container.GetType()) is Style implicitStyle)
			style = implicitStyle;
		if (style != null && ReferenceEquals(container.ReadLocalValue(StyleProperty), DependencyProperty.UnsetValue))
			container.Style = style;
	}
}
