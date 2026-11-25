namespace System.Windows;

public partial class Window
{
	// WPF opens a Manual window whose Width/Height are NaN at the OS default size (Win32 CW_USEDEFAULT), not at
	// its content's size. Win32 picks that size itself, so this layer uses one deterministic stand-in: three
	// quarters of the primary monitor work area per axis, floored, clamped by Min/Max.
	internal static double DefaultExtent(double workArea) => Math.Floor(workArea * 3 / 4);

	/// <summary>
	/// Sets the GTK default size per axis: the explicit Width/Height, -1 (content natural size) for an axis
	/// <see cref="SizeToContent"/> sizes, and the default extent for a NaN axis. Explicit axes are also pinned
	/// with a size request, because the GTK4 Windows backend ignores a default size alone for them.
	/// </summary>
	void ApplyWindowSize(Gtk.Window gtkWin)
	{
		var stc = SizeToContent;
		bool fitW = stc == SizeToContent.Width || stc == SizeToContent.WidthAndHeight;
		bool fitH = stc == SizeToContent.Height || stc == SizeToContent.WidthAndHeight;
		int w = AxisSize(fitW, Width, MinWidth, MaxWidth, SystemParameters.WorkArea.Width, out bool pinW);
		int h = AxisSize(fitH, Height, MinHeight, MaxHeight, SystemParameters.WorkArea.Height, out bool pinH);
		if (w <= 0 && h <= 0)
			return;
		gtkWin.SetDefaultSize(w, h);
		if (pinW || pinH)
			gtkWin.SetSizeRequest(pinW ? w : -1, pinH ? h : -1);
	}

	static int AxisSize(bool fit, double size, double min, double max, double workArea, out bool pinned)
	{
		pinned = false;
		if (fit)
			return -1;
		if (!double.IsNaN(size))
		{
			pinned = true;
			return (int)Math.Round(size);
		}
		return (int)Math.Round(Math.Max(min, Math.Min(DefaultExtent(workArea), max)));
	}
}
