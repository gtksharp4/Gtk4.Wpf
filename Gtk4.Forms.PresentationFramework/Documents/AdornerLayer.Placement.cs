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

	/// <summary>Repositions every placed adorner; catches GTK moves that raise no WPF layout event.</summary>
	internal void RepositionAll()
	{
		foreach (var adorner in new List<Adorner>(_placed.Keys))
			Reposition(adorner);
	}

	void Place(Adorner adorner)
	{
		var overlay = _gtkOverlay;
		if (overlay == null || _placed.ContainsKey(adorner)) return;
		var widget = adorner.Widget;
		widget.Halign = Gtk.Align.Start;
		widget.Valign = Gtk.Align.Start;
		var tracker = new Tracker { Reposition = () => Reposition(adorner) };
		adorner.SuppressCanTarget = true;
		adorner.RefreshCanTarget();
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
		adorner.SuppressCanTarget = false;
		adorner.RefreshCanTarget();
	}

	// Covers the adorned element's border box in overlay coordinates. A placed widget is not pickable (GTK
	// would route every pick over the adorned element to it), so adorner-drawn interactive content is a gap.
	// An adorned element that is unmapped or has no widget hides the adorner and leaves its geometry alone.
	void Reposition(Adorner adorner)
	{
		var overlay = _gtkOverlay;
		if (overlay == null || !_placed.ContainsKey(adorner)) return;
		var widget = adorner.Widget;
		var adorned = adorner.AdornedElement;
		Graphene.Rect? bounds = null;
		if (adorned.HasBackingWidget && adorned.Widget.IsMapped && adorned.Widget.ComputeBounds(overlay, out var b))
			bounds = b;
		if (bounds == null)
		{
			widget.Visible = false;
			return;
		}
		widget.MarginStart = (int)Math.Round(bounds.X);
		widget.MarginTop = (int)Math.Round(bounds.Y);
		widget.SetSizeRequest((int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height));
		widget.Visible = VisibilityGtkMap.Map(adorner.WidgetVisibility).WidgetVisible;
	}
}
