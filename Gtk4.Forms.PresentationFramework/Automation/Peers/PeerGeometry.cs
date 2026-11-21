namespace System.Windows.Automation.Peers;

static class PeerGeometry
{
	// Bounds in the root window's coordinate space. GTK 4 has no screen position for a toplevel.
	internal static Rect Bounds(UIElement element)
	{
		if (!element.HasBackingWidget) return Rect.Empty;
		var widget = element.Widget;
		if (widget.Root is not Gtk.Widget root) return Rect.Empty;
		double x = 0, y = 0;
		if (root != widget && !widget.TranslateCoordinates(root, 0, 0, out x, out y))
			return Rect.Empty;
		return new Rect(x, y, widget.Width, widget.Height);
	}

	internal static Rect Bounds(Gtk.Widget? widget)
	{
		if (widget == null || widget.Root is not Gtk.Widget root) return Rect.Empty;
		double x = 0, y = 0;
		if (root != widget && !widget.TranslateCoordinates(root, 0, 0, out x, out y)) return Rect.Empty;
		return new Rect(x, y, widget.Width, widget.Height);
	}

	internal static bool Contains(Rect rect, Point point) =>
		!rect.IsEmpty && point.X >= rect.X && point.X <= rect.X + rect.Width
		&& point.Y >= rect.Y && point.Y <= rect.Y + rect.Height;
}
