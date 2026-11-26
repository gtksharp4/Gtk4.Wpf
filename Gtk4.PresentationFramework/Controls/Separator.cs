namespace System.Windows.Controls;

/// <summary>
/// A thin visual separator line backed by <see cref="Gtk.Separator"/>.
/// </summary>
public class Separator : Control
{
	// ── Orientation DP ───────────────────────────────────────────────────────

	/// <summary>Orientation of the separator line.</summary>
	public static readonly DependencyProperty OrientationProperty =
		DependencyProperty.Register(
			nameof(Orientation),
			typeof(Orientation),
			typeof(Separator),
			new PropertyMetadata(Orientation.Horizontal));

	/// <summary>Gets or sets the orientation.</summary>
	public Orientation Orientation
	{
		get => (Orientation?)GetValue(OrientationProperty) ?? Orientation.Horizontal;
		set => SetValue(OrientationProperty, value);
	}

	// ── GTK backing ─────────────────────────────────────────────────────────

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		var gtkOrientation = Orientation == Orientation.Vertical
			? Gtk.Orientation.Vertical
			: Gtk.Orientation.Horizontal;
		return new Gtk.Separator(gtkOrientation);
	}
}