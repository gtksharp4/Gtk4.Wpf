namespace System.Windows;

/// <summary>
/// Injectable drag-and-drop backend. The default is <see cref="GtkDragDropBackend"/>;
/// tests inject <see cref="InProcessDragDropBackend"/> to avoid any display/GTK dependency.
/// </summary>
/// <remarks>
/// <b>Headless degrade:</b> when no GTK display is available (CI, unit tests) the default
/// <see cref="GtkDragDropBackend"/> falls back to <see cref="InProcessDragDropBackend"/> so
/// <see cref="DragDrop.DoDragDrop"/> returns a result without throwing.
/// </remarks>
internal interface IDragDropBackend
{
	/// <summary>
	/// Begins a drag-and-drop operation.
	/// </summary>
	/// <param name="source">The element initiating the drag.</param>
	/// <param name="data">The data payload.</param>
	/// <param name="allowedEffects">Effects permitted by the source.</param>
	/// <returns>The resulting <see cref="DragDropEffects"/> negotiated with the drop target.</returns>
	DragDropEffects DoDragDrop(DependencyObject source, IDataObject data, DragDropEffects allowedEffects);
}
