namespace System.Windows.Controls;

public partial class TextBlock
{
	// A hosted widget is positioned by its start/top margins (overlay children are allocated at their margin
	// box), so no layer widget covers the label and picks reach the label (links) or the child.
	// A nested TextBlock keeps its own padding, which lives in those same widget margins.
	// The margin the placement added on top of the child's own padding; its measured box must not include it.
	static (int X, int Y) HostedOffset(UIElement child)
	{
		var w = child.Widget;
		if (w.Parent is not Gtk.Overlay) return (0, 0);
		var pad = child is TextBlock tb ? tb.Padding : default;
		return (w.MarginStart - (int)Math.Round(pad.Left), w.MarginTop - (int)Math.Round(pad.Top));
	}

	static void PlaceHostedWidget(UIElement child, int x, int y)
	{
		var w = child.Widget;
		if (w.Halign != Gtk.Align.Start) w.Halign = Gtk.Align.Start;
		if (w.Valign != Gtk.Align.Start) w.Valign = Gtk.Align.Start;
		var pad = child is TextBlock tb ? tb.Padding : default;
		int left = x + (int)Math.Round(pad.Left), top = y + (int)Math.Round(pad.Top);
		if (w.MarginStart != left) w.MarginStart = left;
		if (w.MarginTop != top) w.MarginTop = top;
	}

	static void ResetHostedWidget(UIElement child)
	{
		var w = child.Widget;
		w.Halign = Gtk.Align.Fill;
		w.Valign = Gtk.Align.Fill;
		w.MarginStart = 0;
		w.MarginTop = 0;
		if (child is TextBlock tb) tb.ApplyPadding();
	}
}
