namespace System.Windows;

public abstract partial class FrameworkElement
{
	// The size request and alignment are pushed to the GTK widget when it is created; a later Width/Height change
	// (a binding that resolves after layout) must re-push them. The request is cleared only when this code had
	// set it (the old value was explicit), so a request another owner placed on the widget is left alone.
	void SyncLayoutToWidget(DependencyPropertyChangedEventArgs e)
	{
		if (!HasBackingWidget || this is Window) return;
		if (e.Property != WidthProperty && e.Property != HeightProperty) return;
		if (e.OldValue is double old && !double.IsNaN(old)
			&& double.IsNaN(Width) && double.IsNaN(Height))
			Widget.SetSizeRequest(-1, -1);
		ApplyLayoutToWidget(Widget);
		InvalidateMeasure();
	}
}
