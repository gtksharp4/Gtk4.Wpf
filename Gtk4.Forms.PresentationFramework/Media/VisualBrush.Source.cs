namespace System.Windows.Media;

public partial class VisualBrush
{
	Gtk.WidgetPaintable? _paintable;
	UIElement? _watched;
	Cairo.ImageSurface? _cache;
	bool _dirty = true;
	bool _rendering;
	bool _scheduled;

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == VisualProperty)
			Watch(e.NewValue as UIElement);
	}

	void Watch(UIElement? element)
	{
		if (ReferenceEquals(element, _watched)) return;
		if (_paintable != null)
		{
			_paintable.InvalidateContents -= OnSourceInvalidated;
			_paintable.InvalidateSize -= OnSourceInvalidated;
			_paintable.Dispose();
			_paintable = null;
		}
		ReleaseHost();
		_watched = element;
		_dirty = true;
		if (element == null) return;
		_paintable = new Gtk.WidgetPaintable(element.Widget);
		_paintable.InvalidateContents += OnSourceInvalidated;
		_paintable.InvalidateSize += OnSourceInvalidated;
	}

	// One Changed per main-loop turn; ignored while this brush renders its own source (WPF shows the last frame).
	void OnSourceInvalidated(object? sender, EventArgs e)
	{
		if (_rendering) return;
		_dirty = true;
		if (_scheduled || IsFrozen) return;
		_scheduled = true;
		GLib.Idle.Add(GLib.Priority.DefaultIdle, () =>
		{
			_scheduled = false;
			if (!IsFrozen) OnChanged();
			return false;
		});
	}

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

	void LayoutDetached(UIElement element)
	{
		if (!AutoLayoutContent || element.VisualParent != null) return;
		var size = element.RenderSize;
		if (size.Width > 0 && size.Height > 0) return;
		element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		element.Arrange(new Rect(new Point(0, 0), element.DesiredSize));
		HostDetached(element);
	}

	Cairo.ImageSurface? RenderSource(UIElement element)
	{
		if (_rendering) return _cache;
		LayoutDetached(element);
		var size = element.RenderSize;
		int w = (int)Math.Ceiling(size.Width), h = (int)Math.Ceiling(size.Height);
		if (w <= 0 || h <= 0 || _paintable == null) return null;
		if (!_dirty && _cache != null && _cache.Width == w && _cache.Height == h) return _cache;

		_rendering = true;
		try
		{
			using var snapshot = new Gtk.Snapshot();
			_paintable.Snapshot(snapshot, w, h);
			using var node = snapshot.FreeToNode();
			if (node == null) return null;
			_cache?.Dispose();
			_cache = new Cairo.ImageSurface(Cairo.Format.Argb32, w, h);
			using var cr = new Cairo.Context(_cache);
			node.Draw(cr);
			_dirty = false;
			return _cache;
		}
		finally { _rendering = false; }
	}
}
