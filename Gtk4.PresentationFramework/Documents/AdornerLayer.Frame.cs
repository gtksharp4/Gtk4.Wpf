namespace System.Windows.Documents;

public sealed partial class AdornerLayer
{
	Gdk.FrameClock? _frameClock;

	/// <summary>
	/// Repositions this layer's adorners on every frame layout of the overlay's toplevel. A Window's
	/// implicit layer is driven by the window itself; <see cref="AdornerDecorator"/> layers use this.
	/// </summary>
	internal void HookFrameLayout(Gtk.Widget overlay)
	{
		overlay.Realized += (_, _) => AttachClock(overlay);
		overlay.Unrealized += (_, _) => DetachClock();
		if (overlay.IsRealized) AttachClock(overlay);
	}

	void AttachClock(Gtk.Widget overlay)
	{
		DetachClock();
		_frameClock = overlay.FrameClock;
		if (_frameClock != null) _frameClock.Layout += OnFrameLayout;
	}

	void DetachClock()
	{
		if (_frameClock == null) return;
		_frameClock.Layout -= OnFrameLayout;
		_frameClock = null;
	}

	void OnFrameLayout(object? sender, EventArgs e) => RepositionAll();
}
