namespace System.Windows.Media;

public partial class VisualBrush
{
	HostLease? _lease;

	void HostDetached(UIElement element)
	{
		if (_lease is { Alive: true } || element.VisualParent != null) return;
		_lease = DetachedSourceHost.Acquire(this, element, _link);
		if (_lease == null) return;
		// The first frame is not mapped yet; ask for a repaint once it is.
		GLib.Timeout.Add(100, new GLib.TimeoutHandler(() => { OnSourceInvalidated(); return false; }));
	}

	void ReleaseHost()
	{
		DetachedSourceHost.Release(_lease);
		_lease = null;
	}
}
