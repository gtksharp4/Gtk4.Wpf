namespace System.Windows.Controls;

public partial class TextBlock
{
	Gtk.Overlay NewInlineOverlay()
	{
		var overlay = new Gtk.Overlay();
		overlay.AddTickCallback((w, clock) =>
		{
			RefreshInlineObjects();
			PlaceInlineChildren();
			return true;
		});
		return overlay;
	}

	/// <summary>
	/// Makes the overlay the root of a label-rooted widget that already exists. The label keeps its controllers and
	/// appearance; only the slot-level layout (alignment, expand, size request) moves to the new root.
	/// </summary>
	void PromoteToInlineHost()
	{
		var overlay = NewInlineOverlay();
		var root = _label;
		overlay.Halign = root.Halign;
		overlay.Valign = root.Valign;
		overlay.Hexpand = root.Hexpand;
		overlay.Vexpand = root.Vexpand;
		root.GetSizeRequest(out int w, out int h);
		overlay.SetSizeRequest(w, h);
		if (!ReplaceRootWidget(overlay)) return;
		root.Halign = Gtk.Align.Fill;
		root.Valign = Gtk.Align.Fill;
		root.Hexpand = false;
		root.Vexpand = false;
		root.SetSizeRequest(-1, -1);
		overlay.Child = root;
		_inlineOverlay = overlay;
	}
}
