using System.Windows.Documents;

namespace System.Windows;

public partial class Window
{
	/// <summary>
	/// The window's implicit adorner layer, found by <see cref="AdornerLayer.GetAdornerLayer"/> for any
	/// element under this window that has no nearer <see cref="AdornerDecorator"/>.
	/// </summary>
	/// <remarks>
	/// WPF's default Window template is Border > AdornerDecorator > ContentPresenter. Here the content
	/// is still the window's direct logical and visual child (a real decorator would change
	/// LogicalParent, inheritance and ElementName scopes), so <c>VisualTreeHelper.GetChild(window, 0)</c>
	/// is the content rather than the template Border. The layer is neither a logical nor a visual child.
	/// </remarks>
	internal AdornerLayer ImplicitAdornerLayer { get; } = new AdornerLayer();

	Gtk.Overlay? _contentOverlay;

	// Hosts the content in an overlay so adorner widgets render above it.
	Gtk.Overlay CreateContentOverlay()
	{
		_contentOverlay = new Gtk.Overlay();
		ImplicitAdornerLayer.GtkOverlay = _contentOverlay;
		if (Content is UIElement content)
			_contentOverlay.Child = content.Widget;
		return _contentOverlay;
	}

	// Content changed after the window widget exists: swap the overlay's main child.
	void SetOverlayContent(object? newContent)
	{
		if (_contentOverlay == null) return;
		_contentOverlay.Child = (newContent as UIElement)?.Widget;
	}
}
