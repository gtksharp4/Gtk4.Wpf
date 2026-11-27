using System.Collections.Generic;

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
/// actually lay out. Once an element is mapped the GTK allocation is the only writer of
/// <see cref="UIElement.RenderSize"/> (the managed <see cref="UIElement.Arrange"/> no longer
/// overwrites it).
/// </remarks>
internal static class GtkAllocationSync
{
	/// <summary>Walks <paramref name="root"/>'s tree pre-order and syncs each mapped widget's size.</summary>
	internal static void Sync(UIElement root)
	{
		// Phase 1 records every changed size and refreshes Grid track sizes, so a SizeChanged handler on an
		// ancestor already sees the final RowDefinition.ActualHeight of descendants. Phase 2 notifies top-down.
		List<(FrameworkElement Element, Size Size)>? changed = null;
		foreach (var element in LifecycleBroadcaster.EnumerateAttached(root))
		{
			if (element is not FrameworkElement fe || !fe.IsLoaded || !fe.HasBackingWidget)
				continue;
			var widget = fe.Widget;
			if (!widget.IsMapped)
				continue;
			var allocated = BorderBoxSize(widget);
			// A stretched shape's RenderSize is the stretched size; the rest of its slot is only the alignment offset.
			if (fe is Shapes.Shape { Stretch: not Media.Stretch.None } shape)
				allocated = shape.UpdateStretchMatrix(allocated);
			if (allocated == fe.RenderSize)
				continue;
			(changed ??= new()).Add((fe, allocated));
			if (fe is Controls.Grid grid)
				grid.PublishActualSizes(allocated);
		}
		if (changed == null)
			return;
		foreach (var (element, size) in changed)
			element.SetRenderSizeAndNotify(size);
	}

	/// <summary>
	/// The widget's border box — its allocation minus CSS margin only, i.e. including its CSS
	/// border and padding — which is what WPF's <c>RenderSize</c> (arranged size minus
	/// <c>Margin</c>) measures. <c>gtk_widget_get_width/height</c> return the content box (border
	/// and padding removed), so <c>gtk_widget_compute_bounds</c> against the widget itself is used.
	/// </summary>
	internal static Size BorderBoxSize(Gtk.Widget widget)
	{
		if (widget.ComputeBounds(widget, out var bounds))
			return new Size(bounds.Width, bounds.Height);
		return new Size(widget.Width, widget.Height);
	}
}
