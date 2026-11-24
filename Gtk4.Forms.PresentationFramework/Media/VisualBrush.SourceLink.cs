namespace System.Windows.Media;

/// <summary>
/// A brush's weak subscription to its source widget's paintable, so the native signal closures do not keep a
/// dropped brush alive. A dead owner disconnects the link on its next invalidation (or from the host sweep).
/// </summary>
sealed class SourceLink
{
	readonly WeakReference<VisualBrush> _brush;
	bool _disposed;

	internal SourceLink(VisualBrush brush, UIElement element)
	{
		_brush = new WeakReference<VisualBrush>(brush);
		Paintable = new Gtk.WidgetPaintable(element.Widget);
		Paintable.InvalidateContents += OnInvalidated;
		Paintable.InvalidateSize += OnInvalidated;
	}

	internal Gtk.WidgetPaintable Paintable { get; }

	void OnInvalidated(object? sender, EventArgs e)
	{
		if (_brush.TryGetTarget(out var brush))
			brush.OnSourceInvalidated();
		else
			GLib.Idle.Add(GLib.Priority.DefaultIdle, () => { Dispose(); return false; });
	}

	internal void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		Paintable.InvalidateContents -= OnInvalidated;
		Paintable.InvalidateSize -= OnInvalidated;
		Paintable.Dispose();
	}
}
