namespace System.Windows.Input;

/// <summary>Pure mapping from GTK scroll deltas to WPF mouse-wheel deltas.</summary>
internal static class MouseWheelMap
{
	/// <summary>
	/// Converts a GTK vertical scroll delta to a WPF <see cref="MouseWheelEventArgs.Delta"/>.
	/// GTK <paramref name="dy"/> is positive when scrolling down (towards the user) and one
	/// discrete notch is 1.0; WPF's delta is positive away from the user and one notch is
	/// <see cref="Mouse.MouseWheelDeltaForOneLine"/> (120). Smooth (fractional) deltas scale
	/// proportionally and are rounded.
	/// </summary>
	internal static int ToDelta(double dy) =>
		(int)System.Math.Round(-dy * Mouse.MouseWheelDeltaForOneLine);
}
