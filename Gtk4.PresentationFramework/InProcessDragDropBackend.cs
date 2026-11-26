namespace System.Windows;

/// <summary>
/// No-op drag-and-drop backend used in headless tests and CI environments.
/// <see cref="DoDragDrop"/> returns <see cref="DragDropEffects.None"/> immediately
/// without touching any native APIs.
/// </summary>
internal sealed class InProcessDragDropBackend : IDragDropBackend
{
	/// <summary>
	/// Gets or sets the result that <see cref="DoDragDrop"/> returns.
	/// Tests set this to simulate a specific drop outcome.
	/// </summary>
	internal DragDropEffects SimulatedResult { get; set; } = DragDropEffects.None;

	/// <inheritdoc/>
	public DragDropEffects DoDragDrop(
		DependencyObject source,
		IDataObject data,
		DragDropEffects allowedEffects) => SimulatedResult & allowedEffects;
}
