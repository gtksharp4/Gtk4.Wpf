namespace System.Windows.Input;

public static partial class Mouse
{
	/// <summary>
	/// Sets the cursor shown for the element under the pointer (the last element the pointer reported over when
	/// none is directly over). <see cref="OverrideCursor"/> wins over <paramref name="cursor"/>. A <c>null</c>
	/// cursor restores the widget's default.
	/// </summary>
	/// <returns><c>false</c> when there is no element under the pointer.</returns>
	public static bool SetCursor(Cursor? cursor)
	{
		var target = DirectlyOver as UIElement ?? _lastPositionSource;
		if (target == null || !target.HasBackingWidget) return false;
		target.Widget.CursorFromName = (OverrideCursor ?? cursor)?.GdkName!;
		return true;
	}

	/// <summary>
	/// Re-evaluates the mouse-over state for the current pointer position. GTK recomputes enter/leave crossings
	/// itself and exposes no way to synthesise them, so this is a documented no-op.
	/// </summary>
	public static void Synchronize() { }
}
