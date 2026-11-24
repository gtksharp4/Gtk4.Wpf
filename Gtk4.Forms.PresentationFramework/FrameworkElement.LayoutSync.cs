namespace System.Windows;

public abstract partial class FrameworkElement
{
	// The size request and alignment are pushed to the GTK widget when it is created; a later Width/Height change
	// (a binding that resolves after layout) must re-push them.
	void SyncLayoutToWidget(DependencyPropertyChangedEventArgs e)
	{
		if (!HasBackingWidget || this is Window) return;
		if (e.Property != WidthProperty && e.Property != HeightProperty) return;
		Widget.SetSizeRequest(-1, -1);
		ApplyLayoutToWidget(Widget);
		InvalidateMeasure();
	}
}
