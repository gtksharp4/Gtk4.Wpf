using System.Windows.Controls;
using System.Windows.Input;

namespace System.Windows;

// Access keys (WPF's PostProcessInput on unhandled TextInput / SystemText). Alt+key is taken in the capture
// phase so GTK's own mnemonic activation never fires a second Click; a plain key fires only when the
// focused element leaves its TextInput unhandled (CoreCompatibilityPreferences.IsAltKeyRequiredInAccessKeyDefaultScope
// is false by default).
public partial class Window
{
	internal Gtk.EventControllerKey? _accessKeyController;

	void AttachAccessKeyController(Gtk.Window window)
	{
		var controller = new Gtk.EventControllerKey { PropagationPhase = Gtk.PropagationPhase.Capture };
		controller.KeyPressed += (_, args) =>
		{
			if (HandleAccessKeyval(args.Keyval, args.State)) args.RetVal = true;
		};
		window.AddController(controller);
		_accessKeyController = controller;
		AddHandler(TextInputEvent, new TextCompositionEventHandler(OnAccessKeyTextInput));
	}

	/// <summary>
	/// The window controller's press handler: Alt+key (no Ctrl or Windows) goes to the access-key manager,
	/// as does a plain key while nothing has focus. Returns true when the key is registered in this window,
	/// which stops GTK's built-in mnemonic.
	/// </summary>
	internal bool HandleAccessKeyval(uint keyval, Gdk.ModifierType state)
	{
		var mods = GdkKeyMap.FromGdkModifier(state);
		if ((mods & (ModifierKeys.Control | ModifierKeys.Windows)) != 0) return false;
		bool alt = (mods & ModifierKeys.Alt) != 0;
		if (!alt && FocusedHere() != null) return false;
		return ProcessAccessKey(KeyvalToText(keyval));
	}

	void OnAccessKeyTextInput(object sender, TextCompositionEventArgs e)
	{
		if (e.Handled) return;
		if ((Keyboard.Modifiers & (ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Windows)) != 0) return;
		if (ConsumesText(e.OriginalSource)) return;
		if (ProcessAccessKey(e.Text)) e.Handled = true;
	}

	bool ProcessAccessKey(string? text)
	{
		if (string.IsNullOrEmpty(text) || new System.Globalization.StringInfo(text!).LengthInTextElements != 1)
			return false;
		if (!AccessKeyManager.IsKeyRegistered(this, text!)) return false;
		return AccessKeyManager.ProcessKey(this, text!, false);
	}

	// A text editor consumes plain letters; WPF's TextBox handles TextInput itself, this layer's does not.
	static bool ConsumesText(object? source) =>
		source is TextBox or PasswordBox or RichTextBox or ComboBox { IsEditable: true };

	static string? KeyvalToText(uint keyval)
	{
		if (keyval >= 0x01000000) keyval -= 0x01000000;
		else if (keyval >= 0xff00) return null;
		if (keyval < 0x20 || keyval > 0x10ffff || (keyval >= 0xd800 && keyval <= 0xdfff)) return null;
		return char.ConvertFromUtf32((int)keyval);
	}
}
