using System.Collections;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace System.Windows;

public partial class Window
{
	const uint GdkKeyReturn = 0xff0d;
	const uint GdkKeyKpEnter = 0xff8d;
	const uint GdkKeyEscape = 0xff1b;

	void InitDefaultButtons()
	{
		AddHandler(KeyDownEvent, new KeyEventHandler(OnAcceleratorKeyDown));
		AddHandler(GotFocusEvent, new RoutedEventHandler((_, _) => RefreshDefaulted(this)));
		AddHandler(LostFocusEvent, new RoutedEventHandler((_, _) => RefreshDefaulted(this)));
	}

	// With no focused element nothing routes a KeyDown, so a window controller covers that case. A
	// focused element gets its KeyDown first and the window clicks the button only if it left it
	// unhandled (OnAcceleratorKeyDown).
	void AttachAcceleratorController(Gtk.Window window)
	{
		var controller = new Gtk.EventControllerKey { PropagationPhase = Gtk.PropagationPhase.Capture };
		controller.KeyPressed += (_, args) =>
		{
			if (HandleAcceleratorKeyval(args.Keyval, args.State)) args.RetVal = true;
		};
		window.AddController(controller);
		_acceleratorController = controller;
	}

	internal Gtk.EventControllerKey? _acceleratorController;

	// Keyboard.FocusedElement is process-wide: an element in another window (or left behind by a closed one)
	// is no focus here. An element with no window (inside a Popup, which has no parent) counts as here.
	IInputElement? FocusedHere()
		=> Keyboard.FocusedElement is DependencyObject d && (GetWindow(d) is not Window w || ReferenceEquals(w, this))
			? Keyboard.FocusedElement : null;

	internal bool HandleAcceleratorKeyval(uint keyval, Gdk.ModifierType state)
	{
		if (FocusedHere() != null) return false;
		bool enter = keyval == GdkKeyReturn || keyval == GdkKeyKpEnter;
		if (!enter && keyval != GdkKeyEscape) return false;
		if (GdkKeyMap.FromGdkModifier(state) != ModifierKeys.None) return false;
		return ClickAccelerator(enter, null);
	}

	// Enter and Esc reach the window after the focused element declined them. A focused button, a
	// multi-line editor or an open drop-down keeps them for itself.
	void OnAcceleratorKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Handled || e.Modifiers != ModifierKeys.None) return;
		bool enter = e.Key == Key.Enter;
		if (!enter && e.Key != Key.Escape) return;
		if (ClickAccelerator(enter, e.Source as IInputElement ?? FocusedHere())) e.Handled = true;
	}

	static bool KeepsKey(bool enter, IInputElement? element) => element switch
	{
		ButtonBase => enter,
		TextBox { AcceptsReturn: true } => enter,
		RichTextBox => enter,
		ComboBox { IsDropDownOpen: true } => true,
		_ => false,
	};

	bool ClickAccelerator(bool enter, IInputElement? focus)
	{
		if (KeepsKey(enter, focus) || KeepsKey(enter, FocusedHere())) return false;
		var button = FindButton(this, enter);
		if (button == null) return false;
		button.ClickFromAccelerator();
		return true;
	}

	static Button? FindButton(DependencyObject root, bool isDefault)
	{
		if (root is Button b && (isDefault ? b.IsDefault : b.IsCancel) && b.IsEnabled
			&& b.Visibility == Visibility.Visible)
			return b;
		foreach (var child in LogicalTreeHelper.GetChildren(root))
		{
			if (child is not DependencyObject d) continue;
			var found = FindButton(d, isDefault);
			if (found != null) return found;
		}
		return null;
	}

	static void RefreshDefaulted(DependencyObject root)
	{
		if (root is Button b) b.UpdateIsDefaulted();
		foreach (var child in LogicalTreeHelper.GetChildren(root))
		{
			if (child is DependencyObject d) RefreshDefaulted(d);
		}
	}
}
