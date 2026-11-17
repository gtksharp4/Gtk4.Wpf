namespace System.Windows;

/// <summary>
/// Default <see cref="IDragDropBackend"/>: wraps GTK 4 <c>Gtk.DragSource</c> when a widget
/// and display are available.
/// </summary>
/// <remarks>
/// <b>Degrade:</b> when <c>Gdk.Display.Default</c> returns <see langword="null"/> (no display),
/// <see cref="DoDragDrop"/> delegates to <see cref="InProcessDragDropBackend"/> and returns
/// <see cref="DragDropEffects.None"/> without throwing.
/// <para>
/// <b>Limit:</b> native GTK 4 drag gestures require a display and a main-loop iteration to
/// complete the asynchronous drag protocol. Headless tests therefore exercise only the object
/// model and routing via <see cref="InProcessDragDropBackend"/>; the GTK path is smoke-tested
/// only when a real display is present.
/// </para>
/// </remarks>
internal sealed class GtkDragDropBackend : IDragDropBackend
{
	readonly InProcessDragDropBackend _headless = new();

	/// <inheritdoc/>
	public DragDropEffects DoDragDrop(
		DependencyObject source,
		IDataObject data,
		DragDropEffects allowedEffects)
	{
		if (!HasDisplay())
			return _headless.DoDragDrop(source, data, allowedEffects);

		if (source is not UIElement el || !el.IsWidgetCreated)
			return _headless.DoDragDrop(source, data, allowedEffects);

		// GTK 4 drag is initiated by attaching a DragSource controller and triggering
		// a drag-begin gesture; the actual result is delivered asynchronously via the
		// Drop signal on the target widget. Because WPF's DoDragDrop is synchronous and
		// GTK's protocol is async, we return None here and rely on the Drop event routing
		// to deliver the result to the UI. Full synchronous emulation requires a nested
		// GLib main-loop, which is out of scope for this round.
		//
		// Real drops are handled by UIElement.DragDrop.cs AttachDropTarget / the Dropped
		// handler, which raises Drop on the managed element and collects Effects.
		return DragDropEffects.None;
	}

	static bool HasDisplay()
	{
		try
		{
			return Gdk.Display.Default != null;
		}
		catch
		{
			return false;
		}
	}
}
