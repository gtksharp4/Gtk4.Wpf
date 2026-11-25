namespace System.Windows.Controls;

public partial class TextBlock
{
	// A hosted widget is positioned by its start/top margins (overlay children are allocated at their margin
	// box), so no layer widget covers the label and picks reach the label (links) or the child.
	// A label-rooted nested TextBlock keeps its own padding, which lives in those same widget margins.
	static Thickness HostedPadding(UIElement child)
		=> child is TextBlock { HostsInlineObjects: false } tb ? tb.Padding : default;

	// The margin the placement added on top of the child's own padding; its measured box must not include it.
	static (int X, int Y) HostedOffset(UIElement child)
	{
		var w = child.Widget;
		if (w.Parent is not Gtk.Overlay) return (0, 0);
		var pad = HostedPadding(child);
		return (w.MarginStart - (int)Math.Round(pad.Left), w.MarginTop - (int)Math.Round(pad.Top));
	}

	static void PlaceHostedWidget(UIElement child, int x, int y)
	{
		var w = child.Widget;
		if (w.Halign != Gtk.Align.Start) w.Halign = Gtk.Align.Start;
		if (w.Valign != Gtk.Align.Start) w.Valign = Gtk.Align.Start;
		var pad = HostedPadding(child);
		int left = x + (int)Math.Round(pad.Left), top = y + (int)Math.Round(pad.Top);
		if (w.MarginStart != left) w.MarginStart = left;
		if (w.MarginTop != top) w.MarginTop = top;
	}

	static void ResetHostedWidget(UIElement child, (Gtk.Align H, Gtk.Align V) align)
	{
		var w = child.Widget;
		w.Halign = align.H;
		w.Valign = align.V;
		w.MarginStart = 0;
		w.MarginTop = 0;
		if (child is TextBlock tb) tb.ApplyPadding();
	}
}
