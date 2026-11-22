using System.Windows.Shell;

namespace System.Windows;

public partial class Window
{
	bool _shellDragWired;

	internal void OnShellChromeChanged(object? sender, EventArgs e) => ApplyShellChrome();

	/// <summary>True when an attached <see cref="WindowChrome"/> has a caption area, so the window draws its own.</summary>
	internal bool HasShellCaption => WindowChrome.GetWindowChrome(this) is { CaptionHeight: > 0 };

	/// <summary>
	/// Maps the attached <see cref="WindowChrome"/> onto the GTK window: a caption area removes the native
	/// decoration and its strip drags the window. Everything else on the chrome is stored only.
	/// </summary>
	internal void ApplyShellChrome()
	{
		if (_window == null) return;
		_window.Decorated = !HasShellCaption && WindowStyle != WindowStyle.None;
		if (_shellDragWired) return;
		_shellDragWired = true;
		var click = new Gtk.GestureClick { Button = 1, PropagationPhase = Gtk.PropagationPhase.Capture };
		click.Pressed += (_, a) => OnShellPressed(a.X, a.Y, click);
		_window.AddController(click);
	}

	void OnShellPressed(double x, double y, Gtk.GestureClick gesture)
	{
		var chrome = WindowChrome.GetWindowChrome(this);
		if (chrome == null || chrome.CaptionHeight <= 0 || y > chrome.CaptionHeight || _window == null) return;
		if (IsHitTestVisibleInChromeAt(x, y)) return;
		RecordPress(gesture.CurrentEventDevice, x, y, gesture.CurrentEventTime);
		try
		{
			DragMove();
		}
		catch (InvalidOperationException)
		{
			// The press was already released: nothing to move.
		}
	}

	// A child that opts in with WindowChrome.IsHitTestVisibleInChrome keeps its own clicks.
	bool IsHitTestVisibleInChromeAt(double x, double y)
	{
		for (var widget = _window!.Pick(x, y, Gtk.PickFlags.Default); widget != null; widget = widget.Parent)
			if (UIElement.OwnerOf(widget) is { } owner && (bool)owner.GetValue(WindowChrome.IsHitTestVisibleInChromeProperty)!)
				return true;
		return false;
	}
}
