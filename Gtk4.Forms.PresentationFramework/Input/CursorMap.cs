namespace System.Windows.Input;

/// <summary>
/// Maps WPF <see cref="CursorType"/> values to GTK 4 / CSS cursor names.
/// Pure static helper; no GTK runtime dependency.
/// </summary>
internal static class CursorMap
{
	/// <summary>
	/// Returns the CSS / freedesktop cursor name accepted by
	/// <c>Gtk.Widget.CursorFromName</c> for the given <paramref name="cursorType"/>.
	/// </summary>
	/// <param name="cursorType">WPF cursor type to convert.</param>
	/// <returns>Lowercase CSS cursor name string.</returns>
	internal static string ToGdkName(CursorType cursorType) => cursorType switch
	{
		CursorType.None        => "none",
		CursorType.Arrow       => "default",
		CursorType.AppStarting => "progress",
		CursorType.Cross       => "crosshair",
		CursorType.Hand        => "pointer",
		CursorType.Help        => "help",
		CursorType.IBeam       => "text",
		CursorType.No          => "not-allowed",
		CursorType.SizeAll     => "move",
		CursorType.SizeNESW    => "nesw-resize",
		CursorType.SizeNS      => "ns-resize",
		CursorType.SizeNWSE    => "nwse-resize",
		CursorType.SizeWE      => "ew-resize",
		CursorType.UpArrow     => "n-resize",
		CursorType.Wait        => "wait",
		CursorType.Pen         => "cell",
		CursorType.ScrollAll   => "all-scroll",
		CursorType.ScrollN     => "n-resize",
		CursorType.ScrollS     => "s-resize",
		CursorType.ScrollE     => "e-resize",
		CursorType.ScrollW     => "w-resize",
		CursorType.ScrollNS    => "ns-resize",
		CursorType.ScrollWE    => "ew-resize",
		CursorType.ScrollNE    => "ne-resize",
		CursorType.ScrollNW    => "nw-resize",
		CursorType.ScrollSE    => "se-resize",
		CursorType.ScrollSW    => "sw-resize",
		CursorType.ArrowCD     => "no-drop",
		_                      => "default",
	};
}
