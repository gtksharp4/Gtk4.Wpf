namespace System.Windows.Input;

/// <summary>Pure mapping from GTK scroll deltas to WPF mouse-wheel deltas.</summary>
internal static class MouseWheelMap
{
	/// <summary>
	/// Surface pixels treated as one wheel notch when GTK reports scrolling in
	/// <c>GDK_SCROLL_UNIT_SURFACE</c> (touchpads/precise scrolling): 48, the distance WPF's
	/// <c>ScrollViewer</c> scrolls per notch (3 wheel lines × 16 px per line).
	/// </summary>
	internal const double SurfacePixelsPerNotch = 48.0;

	/// <summary>
	/// Converts a GTK vertical scroll delta in wheel units to a WPF
	/// <see cref="MouseWheelEventArgs.Delta"/>. GTK <paramref name="dy"/> is positive when
	/// scrolling down (towards the user) and one discrete notch is 1.0; WPF's delta is positive
	/// away from the user and one notch is <see cref="Mouse.MouseWheelDeltaForOneLine"/> (120).
	/// Smooth (fractional) deltas scale proportionally and are rounded.
	/// </summary>
	internal static int ToDelta(double dy) =>
		(int)System.Math.Round(-dy * Mouse.MouseWheelDeltaForOneLine);

	/// <summary>
	/// Unit-aware conversion: <paramref name="surfaceUnits"/> means <paramref name="dy"/> is in
	/// surface pixels (<c>GDK_SCROLL_UNIT_SURFACE</c>), which are first converted to notches at
	/// <see cref="SurfacePixelsPerNotch"/>; otherwise it is in wheel notches
	/// (<c>GDK_SCROLL_UNIT_WHEEL</c>). WPF's delta is always in wheel units, never pixels.
	/// </summary>
	internal static int ToDelta(double dy, bool surfaceUnits) =>
		ToDelta(surfaceUnits ? dy / SurfacePixelsPerNotch : dy);
}
