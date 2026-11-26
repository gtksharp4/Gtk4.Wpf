namespace System.Windows;

/// <summary>
/// Provides static methods to initiate drag-and-drop operations.
/// Mirrors the WPF <c>System.Windows.DragDrop</c> API.
/// </summary>
/// <remarks>
/// <b>Headless degrade:</b> when no GTK display is available (CI, unit tests) the default
/// <see cref="GtkDragDropBackend"/> falls back to <see cref="InProcessDragDropBackend"/>,
/// which returns <see cref="DragDropEffects.None"/> immediately without throwing.
/// <para>
/// <b>Limit:</b> the native GTK 4 drag protocol is asynchronous. <see cref="DoDragDrop"/>
/// initiates the drag but cannot block until the drop completes without a nested GLib main
/// loop. The returned <see cref="DragDropEffects"/> reflects the best available result:
/// <see cref="DragDropEffects.None"/> when headless or when the GTK path cannot determine
/// the outcome synchronously.
/// </para>
/// </remarks>
public static class DragDrop
{
	/// <summary>
	/// Injectable drag-and-drop backend. Defaults to <see cref="GtkDragDropBackend"/>;
	/// replace with <see cref="InProcessDragDropBackend"/> in unit tests.
	/// </summary>
	internal static IDragDropBackend Backend { get; set; } = new GtkDragDropBackend();

	/// <summary>
	/// Initiates a drag-and-drop operation.
	/// </summary>
	/// <param name="source">The <see cref="DependencyObject"/> that is the source of the drag.</param>
	/// <param name="data">
	/// The data to drag. If <paramref name="data"/> does not implement <see cref="IDataObject"/>
	/// it is wrapped in a <see cref="DataObject"/> automatically.
	/// </param>
	/// <param name="allowedEffects">The drag effects the source permits.</param>
	/// <returns>The <see cref="DragDropEffects"/> negotiated with the drop target.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="source"/> or <paramref name="data"/> is <see langword="null"/>.
	/// </exception>
	public static DragDropEffects DoDragDrop(
		DependencyObject source,
		object data,
		DragDropEffects allowedEffects)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(data);

		IDataObject dataObject = data as IDataObject ?? new DataObject(data);
		return Backend.DoDragDrop(source, dataObject, allowedEffects);
	}
}
