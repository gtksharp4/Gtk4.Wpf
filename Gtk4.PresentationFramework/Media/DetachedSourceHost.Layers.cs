using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Media;

static partial class DetachedSourceHost
{
	static readonly Dictionary<Gtk.Overlay, Gtk.Fixed> _layers = new();

	static bool IsLayer(Gtk.Widget? widget) => widget != null && _layers.ContainsValue((Gtk.Fixed)widget);

	static bool AnyWindow() => Overlays().Any();

	static IEnumerable<Gtk.Overlay> Overlays() =>
		Gtk.Window.ListToplevels().Where(w => w.IsMapped).Select(w => Window.ContentOverlayOf(w)).OfType<Gtk.Overlay>();

	// The window that paints the brush when known, else the active one, else the newest mapped window.
	static Gtk.Fixed? EnsureLayer(Gtk.Widget? painterRoot)
	{
		foreach (var dead in _layers.Where(p => p.Value.Parent != p.Key).Select(p => p.Key).ToList())
			_layers.Remove(dead);
		var overlay = Window.ContentOverlayOf(painterRoot as Gtk.Window);
		if (overlay == null || !overlay.IsMapped)
			overlay = Gtk.Window.ListToplevels().Where(w => w.IsMapped && w.IsActive)
				.Select(w => Window.ContentOverlayOf(w)).FirstOrDefault(o => o != null) ?? Overlays().FirstOrDefault();
		if (overlay == null) return null;
		if (_layers.TryGetValue(overlay, out var layer) && layer.Parent == overlay) return layer;
		layer = new Gtk.Fixed { CanTarget = false, CanFocus = false, Focusable = false };
		overlay.AddOverlay(layer);
		overlay.SetMeasureOverlay(layer, false);
		_layers[overlay] = layer;
		return layer;
	}

	static void DropLayer(Gtk.Fixed layer)
	{
		var pair = _layers.FirstOrDefault(p => p.Value == layer);
		if (pair.Key == null) return;
		_layers.Remove(pair.Key);
		if (layer.Parent == pair.Key) pair.Key.RemoveOverlay(layer);
	}
}
