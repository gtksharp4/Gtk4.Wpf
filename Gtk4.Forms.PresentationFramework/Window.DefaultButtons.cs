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

	// A capture-phase controller sees Enter/Esc before GTK's own widgets (an Entry would activate and
	// swallow Return), so the default and cancel buttons work from any focus, or none.
	void AttachAcceleratorController(Gtk.Window window)
	{
		var controller = new Gtk.EventControllerKey { PropagationPhase = Gtk.PropagationPhase.Capture };
		controller.KeyPressed += (_, args) =>
		{
			if (HandleAcceleratorKeyval(args.Keyval, args.State)) args.RetVal = true;
		};
		window.AddController(controller);
	}

	internal bool HandleAcceleratorKeyval(uint keyval, Gdk.ModifierType state)
	{
		bool enter = keyval == GdkKeyReturn || keyval == GdkKeyKpEnter;
		if (!enter && keyval != GdkKeyEscape) return false;
		if (GdkKeyMap.FromGdkModifier(state) != ModifierKeys.None) return false;
		return ClickAccelerator(enter, Keyboard.FocusedElement);
	}

	// Enter and Esc reach the window after the focused element declined them. A focused button or a
	// multi-line TextBox keeps Enter for itself.
	void OnAcceleratorKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Handled || e.Modifiers != ModifierKeys.None) return;
		bool enter = e.Key == Key.Enter;
		if (!enter && e.Key != Key.Escape) return;
		if (ClickAccelerator(enter, e.Source as IInputElement ?? Keyboard.FocusedElement)) e.Handled = true;
	}

	bool ClickAccelerator(bool enter, IInputElement? focus)
	{
		if (enter && (focus is ButtonBase || focus is TextBox { AcceptsReturn: true }
			|| Keyboard.FocusedElement is ButtonBase || Keyboard.FocusedElement is TextBox { AcceptsReturn: true }))
			return false;
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
