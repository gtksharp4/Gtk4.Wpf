namespace System.Windows.Input;

/// <summary>
/// Provides keyboard state and focus services. The managed focus state is updated by
/// <see cref="UIElement"/> when <c>Focus()</c> is called or the GTK focus controller fires.
/// </summary>
public static class Keyboard
{
	// ── Managed focus state ──────────────────────────────────────────────────

	static System.Windows.UIElement? _focusedElement;

	/// <summary>The element that currently holds keyboard focus (may be null).</summary>
	public static System.Windows.IInputElement? FocusedElement => _focusedElement;

	/// <summary>The active modifier keys (updated by GTK key-event controllers).</summary>
	public static ModifierKeys Modifiers { get; internal set; } = ModifierKeys.None;

	/// <summary>
	/// Returns <c>true</c> when <paramref name="key"/> is currently pressed.
	/// Updated by GTK key-event controllers when a realised widget is involved.
	/// </summary>
	public static bool IsKeyDown(Key key) => _pressedKeys.Contains(key);

	static readonly System.Collections.Generic.HashSet<Key> _pressedKeys =
		new System.Collections.Generic.HashSet<Key>();

	/// <summary>
	/// Moves keyboard focus to <paramref name="element"/>; calls
	/// <see cref="UIElement.Focus()"/> and updates the managed focus record.
	/// </summary>
	public static System.Windows.IInputElement? Focus(System.Windows.IInputElement element)
	{
		if (element is System.Windows.UIElement uie)
		{
			uie.Focus();
			return element;
		}
		return null;
	}

	// ── Internal state management (called by UIElement.GtkInput.cs) ──────────

	/// <summary>Records <paramref name="element"/> as the focused element.</summary>
	internal static void SetFocused(System.Windows.UIElement element)
	{
		_focusedElement = element;
	}

	/// <summary>Clears the focused element if it matches <paramref name="element"/>.</summary>
	internal static void ClearFocused(System.Windows.UIElement element)
	{
		if (ReferenceEquals(_focusedElement, element))
			_focusedElement = null;
	}

	/// <summary>Records a key as pressed.</summary>
	internal static void SetKeyDown(Key key) => _pressedKeys.Add(key);

	/// <summary>Records a key as released.</summary>
	internal static void SetKeyUp(Key key) => _pressedKeys.Remove(key);
}