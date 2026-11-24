namespace System.Windows.Controls;

public partial class Image
{
	/// <inheritdoc/>
	protected override void OnWidgetCreated(Gtk.Widget widget)
	{
		base.OnWidgetCreated(widget);
		ApplyAlignment();
	}

	// WPF arranges an Image at its computed size, at the slot's start, never centred in a larger slot. GTK would
	// stretch a Fill-aligned Picture across the slot and centre the pixels, so Stretch.None pins it to its size.
	void ApplyAlignment()
	{
		if (_picture == null)
			return;
		bool natural = Stretch == Media.Stretch.None;
		if (HorizontalAlignment == HorizontalAlignment.Stretch && double.IsNaN(Width))
			_picture.Halign = natural ? Gtk.Align.Start : Gtk.Align.Fill;
		if (VerticalAlignment == VerticalAlignment.Stretch && double.IsNaN(Height))
			_picture.Valign = natural ? Gtk.Align.Start : Gtk.Align.Fill;
	}
}
