namespace System.Windows.Media;

public partial class VisualBrush
{
	Cairo.ImageSurface? _cache;

	// Hosted with no window: nothing invalidates the cache, and the paintable draws nothing without a root.
	bool Bare => _lease is { Layer: null, SelfMapped: true };
	ulong _cacheHash;
	int _retries;

	void ApplyElement(Cairo.Context cr, double width, double height, UIElement element)
	{
		Watch(element);
		var surface = RenderSource(element);
		if (surface == null)
		{
			SetTransparentSource(cr);
			return;
		}
		var content = new Rect(0, 0, surface.Width, surface.Height);
		ApplyTile(cr, width, height, content, c =>
		{
			c.SetSourceSurface(surface, 0, 0);
			c.Paint();
		});
	}

	// Measures and arranges an unparented source (every render: both are no-ops while the layout is valid)
	// and keeps it hosted so GTK maps and draws it.
	void LayoutDetached(UIElement element)
	{
		if (element.VisualParent != null) return;
		if (AutoLayoutContent)
		{
			element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			element.Arrange(new Rect(new Point(0, 0), element.DesiredSize));
		}
		HostDetached(element);
		// No window parents it, so nothing allocates it: do it here.
		if (_lease is { Layer: null, SelfMapped: true })
			AllocateBare(element.Widget, (int)Math.Ceiling(element.RenderSize.Width),
				(int)Math.Ceiling(element.RenderSize.Height));
	}

	void RetryLater()
	{
		if (_retries++ < 20)
			GLib.Timeout.Add(60, new GLib.TimeoutHandler(() => { OnSourceInvalidated(); return false; }));
	}

	static void AllocateBare(Gtk.Widget widget, int w, int h)
	{
		widget.Measure(Gtk.Orientation.Horizontal, -1, out int minW, out _, out _, out _);
		w = Math.Max(w, minW);
		widget.Measure(Gtk.Orientation.Vertical, w, out int minH, out _, out _, out _);
		widget.Allocate(w, Math.Max(h, minH), -1, null!);
	}

	Cairo.ImageSurface? RenderSource(UIElement element)
	{
		if (_rendering) return _cache;
		EnsureLink(element);
		LayoutDetached(element);
		// Waiting to be moved into a window's layer: nothing is drawable yet.
		if (_lease is { Layer: null, SelfMapped: false })
		{
			RetryLater();
			return _cache;
		}
		var size = element.RenderSize;
		int w = (int)Math.Ceiling(size.Width), h = (int)Math.Ceiling(size.Height);
		if (w <= 0 || h <= 0 || _link == null) return null;
		if (!_dirty && !Bare && _cache != null && _cache.Width == w && _cache.Height == h) return _cache;
		var fresh = Capture(w, h);
		if (fresh == null)
		{
			// A hosted source is not drawable until its first allocation: keep the last frame, look again shortly.
			if (_lease != null) RetryLater();
			return _cache;
		}
		_retries = 0;
		_cacheHash = Hash(fresh);
		_cache?.Dispose();
		_cache = fresh;
		_dirty = false;
		return _cache;
	}

	// Snapshots the source; its own invalidations are ignored meanwhile (WPF shows the last frame).
	Cairo.ImageSurface? Capture(int w, int h)
	{
		_rendering = true;
		try
		{
			using var snapshot = new Gtk.Snapshot();
			if (Bare) DetachedSourceHost.Bare.SnapshotChild(_watched!.Widget, snapshot);
			else _link!.Paintable.Snapshot(snapshot, w, h);
			using var node = snapshot.FreeToNode();
			if (node == null) return null;
			var fresh = new Cairo.ImageSurface(Cairo.Format.Argb32, w, h);
			using (var cr = new Cairo.Context(fresh)) node.Draw(cr);
			return fresh;
		}
		finally { _rendering = false; }
	}

	static ulong Hash(Cairo.ImageSurface surface)
	{
		surface.Flush();
		var data = surface.Data;
		ulong h = 14695981039346656037;
		for (int i = 0; i + 8 <= data.Length; i += 8) h = (h ^ BitConverter.ToUInt64(data, i)) * 1099511628211;
		return h;
	}
}
