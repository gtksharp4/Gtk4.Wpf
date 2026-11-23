namespace System.Windows.Documents;

public sealed partial class AdornerLayer
{
	Gtk.Overlay? _gtkOverlay;

	/// <summary>
	/// The overlay hosting adorner widgets, set by <see cref="AdornerDecorator"/> or
	/// <see cref="Window"/> once its widget exists. Adorners added earlier are placed on assignment.
	/// </summary>
	internal Gtk.Overlay? GtkOverlay
	{
		get => _gtkOverlay;
		set
		{
			if (ReferenceEquals(_gtkOverlay, value)) return;
			UnplaceAll();
			_gtkOverlay = value;
			if (value != null) PlaceAll();
		}
	}

	/// <summary>Places the adorner's widget in the overlay when one is wired.</summary>
	partial void OnAdornerAdded(Adorner adorner)
	{
		if (_gtkOverlay != null) Place(adorner);
	}

	/// <summary>Removes the adorner's widget from the overlay if it was placed.</summary>
	partial void OnAdornerRemoved(Adorner adorner) => Unplace(adorner);
}
