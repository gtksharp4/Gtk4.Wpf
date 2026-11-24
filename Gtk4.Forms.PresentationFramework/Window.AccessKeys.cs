using System.Windows.Controls;
using System.Windows.Input;

namespace System.Windows;

// Access keys (WPF's PostProcessInput on unhandled TextInput / SystemText). Alt+key is taken in the capture
// phase and GTK's own mnemonic matching is switched off (Window.AccessKeys.Mnemonics.cs), so a key never
// activates twice; a plain key fires only when the focused element leaves its TextInput unhandled
// (CoreCompatibilityPreferences.IsAltKeyRequiredInAccessKeyDefaultScope is false by default).
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
		var focused = FocusedHere();
		if (!alt && focused != null) return false;
		return ProcessAccessKey(GdkKeyText.FromKeyval(keyval), focused as UIElement ?? InnermostOpenSubmenu());
	}

	void OnAccessKeyTextInput(object sender, TextCompositionEventArgs e)
	{
		if (e.Handled) return;
		if ((Keyboard.Modifiers & (ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Windows)) != 0) return;
		if (ConsumesText(e.OriginalSource)) return;
		if (ProcessAccessKey(e.Text, e.OriginalSource as UIElement)) e.Handled = true;
	}

	// A registered key is swallowed even when it yields no target, so GTK never moves focus for it.
	bool ProcessAccessKey(string? text, UIElement? sender)
	{
		if (string.IsNullOrEmpty(text) || new System.Globalization.StringInfo(text!).LengthInTextElements != 1)
			return false;
		var result = AccessKeyManager.ProcessKeyForScope(this, text!, false, sender);
		return result != ProcessKeyResult.NoMatch || AccessKeyManager.HasRegistrationIn(this, text!);
	}

	// A text editor consumes plain letters; WPF's TextBox handles TextInput itself, this layer's does not.
	static bool ConsumesText(object? source) =>
		source is TextBox or PasswordBox or RichTextBox or ComboBox { IsEditable: true };

	// The deepest open submenu holds the keyboard when nothing else is focused.
	MenuItem? InnermostOpenSubmenu()
	{
		MenuItem? found = null;
		void Visit(DependencyObject node)
		{
			foreach (var child in LogicalTreeHelper.GetChildren(node))
			{
				if (child is not DependencyObject d) continue;
				if (d is MenuItem { IsSubmenuOpen: true } item) found = item;
				Visit(d);
			}
		}
		Visit(this);
		return found;
	}
}
