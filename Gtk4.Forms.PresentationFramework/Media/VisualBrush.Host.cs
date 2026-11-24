namespace System.Windows.Media;

public partial class VisualBrush
{
	UIElement? _hosted;

	void HostDetached(UIElement element)
	{
		if (element.Widget.Parent != null || _hosted != null) return;
		var size = element.RenderSize;
		_hosted = element;
		DetachedSourceHost.Add(element.Widget, (int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
		// The first frame is not mapped yet; ask for a repaint once it is.
		GLib.Timeout.Add(100, new GLib.TimeoutHandler(() => { OnSourceInvalidated(this, EventArgs.Empty); return false; }));
	}

	void ReleaseHost()
	{
		if (_hosted == null) return;
		DetachedSourceHost.Remove(_hosted.Widget);
		_hosted = null;
	}
}

/// <summary>
/// One process-wide, near-transparent, non-interactive window holding every unparented brush source at its
/// own origin (GTK draws nothing for an unmapped widget). Not a WPF window; released with the last source.
/// </summary>
static class DetachedSourceHost
{
	static Gtk.Window? _window;
	static Gtk.Fixed? _fixed;
	static int _count;

	internal static void Add(Gtk.Widget widget, int w, int h)
	{
		if (_window == null)
		{
			_fixed = new Gtk.Fixed { CanTarget = false, Focusable = false };
			_window = new Gtk.Window { Decorated = false, Resizable = false, Opacity = 0.01, CanTarget = false, Focusable = false };
			_window.Child = _fixed;
		}
		_fixed!.Put(widget, 0, 0);
		_count++;
		var (cw, ch) = (Math.Max(w, _window.DefaultWidth), Math.Max(h, _window.DefaultHeight));
		_window.SetDefaultSize(cw, ch);
		_window.Present();
	}

	internal static void Remove(Gtk.Widget widget)
	{
		if (_window == null || widget.Parent != _fixed) return;
		_fixed!.Remove(widget);
		if (--_count > 0) return;
		_window.Child = null;
		_window.Hide();
		_window = null;
		_fixed = null;
	}
}
