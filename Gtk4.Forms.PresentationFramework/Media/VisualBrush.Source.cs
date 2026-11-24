namespace System.Windows.Media;

public partial class VisualBrush
{
	// How long after our own Changed the owner redraw it causes can still invalidate the source.
	const int EchoMilliseconds = 100;

	SourceLink? _link;
	UIElement? _watched;
	bool _dirty = true;
	bool _rendering;
	bool _scheduled;
	bool _inFlight;
	bool _missed;
	// Set when the brush is painted inside its own source, whose owner redraw echoes each Changed back. Only such
	// a brush is held back; any other follows every change.
	bool _selfRef;
	int _changedAt;

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
		_selfRef = false;
	}

	// Created on the first paint: a source set while the app is still constructing (App.xaml resources) has no
	// GTK runtime to create its widget in yet.
	void EnsureLink(UIElement element) => _link ??= new SourceLink(this, element);

	// One Changed per main-loop turn. Ignored while this brush renders its own source (WPF shows the last frame).
	// A brush painted inside its own source is invalidated by the owner redraw its own Changed causes. That echo
	// is recognised by its cause: invalidations while that redraw is in flight are held back, and once it is done
	// a re-render settles whether the source really changed (Changed again) or not (a fixed point).
	internal void OnSourceInvalidated()
	{
		if (_rendering) return;
		_dirty = true;
		if (_selfRef && _inFlight)
		{
			_missed = true;
			return;
		}
		if (_scheduled || IsFrozen) return;
		_scheduled = true;
		GLib.Idle.Add(GLib.Priority.DefaultIdle, () =>
		{
			_scheduled = false;
			if (!IsFrozen) RaiseOrSettle();
			return false;
		});
	}

	// Once per loop turn, like WPF's per-frame render. An invalidation soon after our own Changed of a brush
	// that paints inside its source is that Changed echoing back: the hold-back takes over.
	void RaiseOrSettle()
	{
		if (_selfRef) { RaiseChanged(); return; }
		bool recent = unchecked(Environment.TickCount - _changedAt) < EchoMilliseconds;
		if (recent && PaintedInsideSource())
		{
			_selfRef = true;
			Hold(true);
			return;
		}
		_changedAt = Environment.TickCount;
		OnChanged();
	}

	void RaiseChanged()
	{
		Hold(false);
		OnChanged();
	}

	// Holds back invalidations while the owner redraw our Changed causes is in flight; afterwards a re-render
	// settles whether the source really changed (Changed again) or not.
	void Hold(bool missed)
	{
		_inFlight = true;
		_missed = missed;
		GLib.Timeout.Add(EchoMilliseconds, new GLib.TimeoutHandler(() =>
		{
			_inFlight = false;
			if (_missed && !IsFrozen && SourceDiffers()) RaiseChanged();
			_missed = false;
			return false;
		}));
	}

	// True when the source now renders differently from the cached frame, which is then replaced by the new one. A source that cannot be captured right
	// now counts as unchanged: a self-containing source would otherwise never settle.
	bool SourceDiffers()
	{
		var size = _watched?.RenderSize ?? Size.Empty;
		int w = (int)Math.Ceiling(size.Width), h = (int)Math.Ceiling(size.Height);
		if (_link == null || w <= 0 || h <= 0) return false;
		if (_cache == null || w != _cache.Width || h != _cache.Height) return true;
		var fresh = Capture(w, h);
		if (fresh == null) return false;
		ulong hash = Hash(fresh);
		if (hash == _cacheHash)
		{
			fresh.Dispose();
			return false;
		}
		_cache?.Dispose();
		_cache = fresh;
		_cacheHash = hash;
		_dirty = false;
		return true;
	}
}
