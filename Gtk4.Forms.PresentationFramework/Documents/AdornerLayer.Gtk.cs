namespace System.Windows.Documents;

public sealed partial class AdornerLayer
{
	// Set by AdornerDecorator after creating the Gtk.Overlay.
	internal Gtk.Overlay? GtkOverlay { get; set; }

	/// <summary>
	/// When a <see cref="Gtk.Overlay"/> is wired, adds the adorner's widget as an
	/// overlay child so it renders above the content.
	/// Widget creation is deferred: adorners added before widget realisation are
	/// placed when the layer is later attached to an overlay (not yet implemented —
	/// noted as a limitation).
	/// </summary>
	partial void OnAdornerAdded(Adorner adorner)
	{
		if (GtkOverlay == null) return;
		if (!adorner.IsWidgetCreated) return;
		GtkOverlay.AddOverlay(adorner.Widget);
	}

	/// <summary>
	/// Removes the adorner's widget from the overlay when present.
	/// </summary>
	partial void OnAdornerRemoved(Adorner adorner)
	{
		if (GtkOverlay == null) return;
		if (!adorner.IsWidgetCreated) return;
		GtkOverlay.RemoveOverlay(adorner.Widget);
	}
}
