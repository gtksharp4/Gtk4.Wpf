namespace System.Windows;

public partial class FrameworkContentElement
{
	/// <summary>
	/// Identifies the ContextMenu property (shared with <see cref="FrameworkElement.ContextMenuProperty"/>).
	/// </summary>
	public static readonly DependencyProperty ContextMenuProperty = FrameworkElement.ContextMenuProperty.AddOwner(
		typeof(FrameworkContentElement), new PropertyMetadata(null));

	/// <summary>
	/// Gets or sets the context menu. Stored only: a flow document is one GTK label, so no menu opens for a
	/// content element.
	/// </summary>
	public Controls.ContextMenu? ContextMenu
	{
		get => (Controls.ContextMenu?)GetValue(ContextMenuProperty);
		set => SetValue(ContextMenuProperty, value);
	}
}
