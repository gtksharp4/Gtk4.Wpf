namespace System.Windows;

/// <summary>
/// Pure render-size comparison used by <see cref="UIElement.RenderSize"/> to decide whether
/// <see cref="UIElement.OnRenderSizeChanged"/> (and so <see cref="FrameworkElement.SizeChanged"/>)
/// fires, and for which axes.
/// </summary>
internal static class SizeChangeMath
{
	const double DblEpsilon = 2.2204460492503131e-016; // smallest e such that 1.0 + e != 1.0

	/// <summary>
	/// Compares <paramref name="old"/> with <paramref name="new"/> per axis. Returns
	/// <see langword="true"/> when at least one axis changed.
	/// </summary>
	internal static bool Compute(Size old, Size @new, out bool widthChanged, out bool heightChanged)
	{
		widthChanged = !AreClose(old.Width, @new.Width);
		heightChanged = !AreClose(old.Height, @new.Height);
		return widthChanged || heightChanged;
	}

	/// <summary>
	/// WPF's <c>DoubleUtil.AreClose</c>: equal, or within a relative tolerance of
	/// <c>DBL_EPSILON</c> scaled by the magnitudes — no tunable epsilon, so only floating-point
	/// rounding noise is ignored.
	/// </summary>
	internal static bool AreClose(double value1, double value2)
	{
		if (value1 == value2) return true; // also handles matching infinities
		double eps = (Math.Abs(value1) + Math.Abs(value2) + 10.0) * DblEpsilon;
		double delta = value1 - value2;
		return -eps < delta && eps > delta;
	}
}
