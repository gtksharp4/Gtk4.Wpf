namespace System.Windows.Media;

public partial class VisualBrush
{
	HostLease? _lease;

	// Re-checked on every render: a source whose hosting window went away is hosted again.
	void HostDetached(UIElement element)
	{
		if (element.VisualParent != null) return;
		if (_lease != null && DetachedSourceHost.IsHealthy(_lease)) return;
		ReleaseHost();
		_lease = DetachedSourceHost.Acquire(this, element, _link, BrushPaint.PaintingRoot);
		if (_lease == null) return;
		// The first frame may not be mapped yet; ask for a repaint once it is.
		GLib.Timeout.Add(100, new GLib.TimeoutHandler(() => { OnSourceInvalidated(); return false; }));
	}

	void ReleaseHost()
	{
		DetachedSourceHost.Release(_lease);
		_lease = null;
	}
}
