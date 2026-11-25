namespace System.Windows.Controls;

public abstract partial class Panel
{
	internal void RemoveChildWidgetForSwap(UIElement child) => RemoveChildWidget(child);

	internal void AddChildWidgetForSwap(UIElement child) => AddChildWidget(child);
}
