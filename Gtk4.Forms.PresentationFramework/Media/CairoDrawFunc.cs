namespace System.Windows;

/// <summary>
/// Wraps a painter in a <see cref="Gtk.DrawingAreaDrawFunc"/> that disposes the callback's
/// <see cref="Cairo.Context"/> when the paint ends. GtkSharp builds a new wrapper (and takes a cairo reference) for
/// every draw and never releases it, so without this each frame leaks one wrapper to the finalizer thread.
/// Disposing only drops the wrapper's own reference; GTK keeps its context.
/// </summary>
static class CairoDrawFunc
{
	/// <summary>Test hook: receives each context after it has been disposed.</summary>
	internal static Action<Cairo.Context>? Disposed;

	internal static Gtk.DrawingAreaDrawFunc Wrap(Action<Cairo.Context, int, int> paint) =>
		(area, cr, width, height) =>
		{
			try
			{
				paint(cr, width, height);
			}
			finally
			{
				cr.Dispose();
				Disposed?.Invoke(cr);
			}
		};
}
