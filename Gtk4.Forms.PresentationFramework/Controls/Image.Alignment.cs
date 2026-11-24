namespace System.Windows.Controls;

public partial class Image
{
	/// <inheritdoc/>
	protected override void OnWidgetCreated(Gtk.Widget widget)
	{
		base.OnWidgetCreated(widget);
		ApplyAlignment();
	}

	// WPF arranges a Stretch-aligned element of natural size smaller than its slot centred (ComputeAlignmentOffset
	// treats Stretch as Center; it falls back to Left/Top only when the content is larger than the slot). GTK would
	// stretch a Fill-aligned Picture across the slot, so a Stretch.None Picture is pinned to its size, centred.
	void ApplyAlignment()
	{
		if (_picture == null)
			return;
		bool natural = Stretch == Media.Stretch.None;
		if (HorizontalAlignment == HorizontalAlignment.Stretch && double.IsNaN(Width))
			_picture.Halign = natural ? Gtk.Align.Center : Gtk.Align.Fill;
		if (VerticalAlignment == VerticalAlignment.Stretch && double.IsNaN(Height))
			_picture.Valign = natural ? Gtk.Align.Center : Gtk.Align.Fill;
	}
}
