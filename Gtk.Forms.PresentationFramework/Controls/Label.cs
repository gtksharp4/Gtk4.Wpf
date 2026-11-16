using System.Windows.Markup;

namespace System.Windows.Controls
{
	/// <summary>
	/// A simple content label backed by <see cref="Gtk.Label"/>.
	/// When <see cref="ContentControl.Content"/> is a string the GTK label text
	/// is updated directly; <see cref="UIElement"/> content is not supported
	/// (use <see cref="Border"/> or a layout panel for that case).
	/// </summary>
	public class Label : ContentControl
	{
		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			_box = new Gtk.Box(Gtk.Orientation.Horizontal, 0);
			_label = new Gtk.Label(Content as string ?? Content?.ToString() ?? string.Empty)
			{
				Xalign = 0f
			};
			_box.Add(_label);
			return _box;
		}
	}
}
