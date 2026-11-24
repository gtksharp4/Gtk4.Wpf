namespace System.Windows.Media;

public partial class VisualBrush
{
	SourceLink? _link;
	UIElement? _watched;
	Cairo.ImageSurface? _cache;
	bool _dirty = true;
	bool _rendering;
	bool _scheduled;
	ulong _cacheHash;
	int _sameStreak;
	long _lastChanged;

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
		ReleaseHost();
		_link?.Dispose();
		_link = null;
		_watched = element;
		_dirty = true;
	}

	// Created on the first paint: a source set while the app is still constructing (App.xaml resources) has no
	// GTK runtime to create its widget in yet.
	void EnsureLink(UIElement element) => _link ??= new SourceLink(this, element);

	// One Changed per main-loop turn. Ignored while this brush renders its own source (WPF shows the last frame).
	// A brush painted inside its own source is invalidated by its own repaint: once two re-renders in a row came out
	// identical, an invalidation right after our own Changed is that echo and is dropped, so the loop reaches a
	// fixed point instead of repainting forever.
	bool Settled => _sameStreak >= 2 && DateTime.UtcNow.Ticks - _lastChanged < 250 * TimeSpan.TicksPerMillisecond;

	internal void OnSourceInvalidated()
	{
		if (_rendering) return;
		_dirty = true;
		if (_scheduled || IsFrozen || Settled) return;
		_scheduled = true;
		GLib.Idle.Add(GLib.Priority.DefaultIdle, () =>
		{
			_scheduled = false;
			if (IsFrozen) return false;
			_lastChanged = DateTime.UtcNow.Ticks;
			OnChanged();
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
	}

	Cairo.ImageSurface? RenderSource(UIElement element)
	{
		if (_rendering) return _cache;
		EnsureLink(element);
		LayoutDetached(element);
		var size = element.RenderSize;
		int w = (int)Math.Ceiling(size.Width), h = (int)Math.Ceiling(size.Height);
		if (w <= 0 || h <= 0 || _link == null) return null;
		if (!_dirty && _cache != null && _cache.Width == w && _cache.Height == h) return _cache;

		_rendering = true;
		try
		{
			using var snapshot = new Gtk.Snapshot();
			_link.Paintable.Snapshot(snapshot, w, h);
			using var node = snapshot.FreeToNode();
			if (node == null) return null;
			var fresh = new Cairo.ImageSurface(Cairo.Format.Argb32, w, h);
			using (var cr = new Cairo.Context(fresh)) node.Draw(cr);
			ulong hash = Hash(fresh);
			_sameStreak = _cache != null && _cache.Width == w && _cache.Height == h && hash == _cacheHash
				? _sameStreak + 1 : 0;
			_cacheHash = hash;
			_cache?.Dispose();
			_cache = fresh;
			_dirty = false;
			return _cache;
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
