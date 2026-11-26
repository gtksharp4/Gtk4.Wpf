using System.Windows.Shell;

namespace System.Windows;

public partial class Window
{
	internal void OnShellChromeChanged(object? sender, EventArgs e) => ApplyShellChrome();

	/// <summary>
	/// True when an attached <see cref="WindowChrome"/> has a caption area, so the window
	/// draws its own.
	/// </summary>
	internal bool HasShellCaption => WindowChrome.GetWindowChrome(this) is { CaptionHeight: > 0 };

	/// <summary>
	/// Maps the attached <see cref="WindowChrome"/> onto the GTK window: a caption area removes the native
	/// decoration and its strip drags the window (see <see cref="OnShellPressed"/>). Everything else on the
	/// chrome is stored only.
	/// </summary>
	internal void ApplyShellChrome()
	{
		if (_window == null) return;
		_window.Decorated = !HasShellCaption && WindowStyle != WindowStyle.None;
	}

	// Called with the press the drag-move controller just recorded.
	internal void OnShellPressed(double x, double y)
	{
		var chrome = WindowChrome.GetWindowChrome(this);
		if (chrome == null || chrome.CaptionHeight <= 0 || y > chrome.CaptionHeight || _window == null) return;
		if (IsHitTestVisibleInChromeAt(x, y)) return;
		try
		{
			DragMove();
		}
		catch (InvalidOperationException)
		{
			// The press was already released: nothing to move.
		}
	}

	// The effective (inherited) WindowChrome.IsHitTestVisibleInChrome of the element under the press.
	internal bool IsHitTestVisibleInChromeAt(double x, double y)
	{
		for (var widget = _window!.Pick(x, y, Gtk.PickFlags.Default); widget != null; widget = widget.Parent)
			if (UIElement.OwnerOf(widget) is { } owner)
				return EffectiveHitTestVisibleInChrome(owner);
		return false;
	}

	// Template parts do not inherit from their templated control, so a part without a local value
	// takes the nearest in-template ancestor's, else the templated parent's (nested templates repeat).
	static bool EffectiveHitTestVisibleInChrome(UIElement element)
	{
		var dp = WindowChrome.IsHitTestVisibleInChromeProperty;
		for (var guard = 0; guard < 64; guard++)
		{
			if (!(element is FrameworkElement fe) || fe.TemplatedParent == null
				|| fe.HasNonInheritedValue(dp))
				return (bool)element.GetValue(dp)!;
			UIElement next = fe.TemplatedParent;
			for (var w = fe.Widget.Parent; w != null; w = w.Parent)
			{
				if (UIElement.OwnerOf(w) is not { } up) continue;
				if (up is FrameworkElement upFe && upFe.TemplatedParent == fe.TemplatedParent) next = up;
				break;
			}
			element = next;
		}
		return false;
	}
}
