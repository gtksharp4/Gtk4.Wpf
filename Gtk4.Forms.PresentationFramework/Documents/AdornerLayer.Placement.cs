using System.Collections.Generic;

namespace System.Windows.Documents;

public sealed partial class AdornerLayer
{
	// Adorners whose widget is currently an overlay child, with the handlers that re-place them.
	readonly Dictionary<Adorner, Tracker> _placed = new();

	sealed class Tracker
	{
		public Action Reposition = null!;

		public void OnSize(object sender, SizeChangedEventArgs e) => Reposition();

		public void OnLayout(object? sender, EventArgs e) => Reposition();
	}

	void PlaceAll()
	{
		foreach (var list in _adorners.Values)
			foreach (var adorner in list.ToArray())
				Place(adorner);
	}

	void UnplaceAll()
	{
		foreach (var adorner in new List<Adorner>(_placed.Keys))
			Unplace(adorner);
	}

	void Place(Adorner adorner)
	{
		var overlay = _gtkOverlay;
		if (overlay == null || _placed.ContainsKey(adorner)) return;
		var widget = adorner.Widget;
		widget.Halign = Gtk.Align.Start;
		widget.Valign = Gtk.Align.Start;
		var tracker = new Tracker { Reposition = () => Reposition(adorner) };
		_placed[adorner] = tracker;
		// Adorners overlay the content; they must never enlarge the window.
		overlay.AddOverlay(widget);
		overlay.SetMeasureOverlay(widget, false);
		var adorned = adorner.AdornedElement;
		if (adorned is FrameworkElement fe) fe.SizeChanged += tracker.OnSize;
		adorned.LayoutUpdated += tracker.OnLayout;
		Reposition(adorner);
	}

	void Unplace(Adorner adorner)
	{
		if (!_placed.TryGetValue(adorner, out var tracker)) return;
		_placed.Remove(adorner);
		var adorned = adorner.AdornedElement;
		if (adorned is FrameworkElement fe) fe.SizeChanged -= tracker.OnSize;
		adorned.LayoutUpdated -= tracker.OnLayout;
		_gtkOverlay?.RemoveOverlay(adorner.Widget);
	}

	// Covers the adorned element: margin = its origin in the overlay's main child, size = its extent.
	void Reposition(Adorner adorner)
	{
		var overlay = _gtkOverlay;
		if (overlay == null || !_placed.ContainsKey(adorner)) return;
		var widget = adorner.Widget;
		var adorned = adorner.AdornedElement;
		int x = 0, y = 0;
		var main = overlay.Child;
		if (main != null && adorned.HasBackingWidget && adorned.Widget.IsMapped && main.IsMapped)
		{
			var origin = new Graphene.Point();
			origin.X = 0;
			origin.Y = 0;
			var ok = adorned.Widget.ComputePoint(main, origin, out var to);
			if (ok)
			{
				x = (int)Math.Round(to.X);
				y = (int)Math.Round(to.Y);
			}
		}
		var size = adorned.RenderSize;
		widget.MarginStart = x;
		widget.MarginTop = y;
		widget.SetSizeRequest((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
	}
}
