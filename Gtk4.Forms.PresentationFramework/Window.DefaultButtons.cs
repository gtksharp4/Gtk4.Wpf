using System.Collections;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace System.Windows;

public partial class Window
{
	void InitDefaultButtons()
	{
		AddHandler(KeyDownEvent, new KeyEventHandler(OnAcceleratorKeyDown));
		AddHandler(GotFocusEvent, new RoutedEventHandler((_, _) => RefreshDefaulted(this)));
		AddHandler(LostFocusEvent, new RoutedEventHandler((_, _) => RefreshDefaulted(this)));
	}

	// Enter and Esc reach the window after the focused element declined them. A focused button or a
	// multi-line TextBox keeps Enter for itself, and keys the window raised on itself are the GTK echo
	// of an event that already bubbled through here.
	void OnAcceleratorKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Handled || ReferenceEquals(e.Source, this) || e.Modifiers != ModifierKeys.None) return;
		bool enter = e.Key == Key.Enter;
		if (!enter && e.Key != Key.Escape) return;
		if (enter && (e.Source is ButtonBase || e.Source is TextBox { AcceptsReturn: true })) return;

		var button = FindButton(this, enter);
		if (button == null) return;
		e.Handled = true;
		button.ClickFromAccelerator();
	}

	static Button? FindButton(DependencyObject root, bool isDefault)
	{
		if (root is Button b && (isDefault ? b.IsDefault : b.IsCancel) && b.IsEnabled && b.Visibility == Visibility.Visible) return b;
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
