namespace System.Windows.Controls;

static class TextWrappingMap
{
	internal static (bool Wrap, Pango.WrapMode Mode) ToLabel(TextWrapping value) => value switch
	{
		TextWrapping.Wrap => (true, Pango.WrapMode.WordChar),
		TextWrapping.WrapWithOverflow => (true, Pango.WrapMode.Word),
		_ => (false, Pango.WrapMode.Word),
	};

	internal static Gtk.WrapMode ToTextView(TextWrapping value) => value switch
	{
		TextWrapping.Wrap => Gtk.WrapMode.WordChar,
		TextWrapping.WrapWithOverflow => Gtk.WrapMode.Word,
		_ => Gtk.WrapMode.None,
	};
}
