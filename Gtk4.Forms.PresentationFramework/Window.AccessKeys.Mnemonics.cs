namespace System.Windows;

// WPF owns access-key activation; GTK only draws the underline. GtkLabel/GtkButton mnemonics (UseUnderline)
// are matched by the window's own capture-phase shortcut controller, which runs before HandleAccessKeyval and
// would activate a second time. Its mnemonic modifier is set to one no keyboard produces, so none matches.
public partial class Window
{
	const Gdk.ModifierType NeverMnemonicModifier = Gdk.ModifierType.HyperMask;

	void DisableGtkMnemonics(Gtk.Window window)
	{
		foreach (var controller in MnemonicControllers(window))
			controller.MnemonicsModifiers = NeverMnemonicModifier;
	}

	internal static System.Collections.Generic.List<Gtk.ShortcutController> MnemonicControllers(Gtk.Window window)
	{
		var found = new System.Collections.Generic.List<Gtk.ShortcutController>();
		var model = window.ObserveControllers();
		for (uint i = 0; i < model.NItems; i++)
			if (model.GetObject(i) is Gtk.ShortcutController shortcuts)
				found.Add(shortcuts);
		return found;
	}
}
