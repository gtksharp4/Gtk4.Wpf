namespace System.Windows;

/// <summary>
/// Copies the live GTK allocation of every loaded, mapped element of a window into its
/// <see cref="UIElement.RenderSize"/> (raising <see cref="FrameworkElement.SizeChanged"/> when it
/// differs).
/// </summary>
/// <remarks>
/// GTK 4 has no per-widget <c>size-allocate</c> signal, and a per-widget tick callback would keep
/// the frame clock running every frame. Instead <see cref="Window"/> runs <see cref="Sync"/> from
/// the toplevel <c>Gdk.FrameClock</c> <c>layout</c> signal, which fires only on frames that
/// actually lay out. Once an element is mapped the GTK allocation therefore wins over the
/// managed <see cref="UIElement.Arrange"/> result.
/// </remarks>
internal static class GtkAllocationSync
{
	/// <summary>Walks <paramref name="root"/>'s tree pre-order and syncs each mapped widget's size.</summary>
	internal static void Sync(UIElement root)
	{
		foreach (var element in LifecycleBroadcaster.EnumeratePreOrder(root))
		{
			if (element is not FrameworkElement fe || !fe.IsLoaded || !fe.HasBackingWidget)
				continue;
			var widget = fe.Widget;
			if (!widget.IsMapped)
				continue;
			var allocated = new Size(widget.Width, widget.Height);
			if (allocated != fe.RenderSize)
				fe.RenderSize = allocated;
		}
	}
}
