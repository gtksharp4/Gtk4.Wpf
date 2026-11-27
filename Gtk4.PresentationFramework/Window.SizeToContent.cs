namespace System.Windows;

public partial class Window
{
	// ── SizeToContent ─────────────────────────────────────────────────────────

	/// <summary>Gets or sets whether the window auto-sizes to fit its content.</summary>
	public static readonly DependencyProperty SizeToContentProperty =
		DependencyProperty.Register(
			nameof(SizeToContent),
			typeof(SizeToContent),
			typeof(Window),
			new PropertyMetadata(SizeToContent.Manual));

	/// <summary>
	/// Gets or sets whether the window auto-sizes to fit its content.
	/// When set to anything other than <see cref="SizeToContent.Manual"/>, the
	/// affected dimension(s) are left at the GTK natural size (no explicit size request),
	/// so the window wraps its content.
	/// </summary>
	public SizeToContent SizeToContent
	{
		get => (SizeToContent)(GetValue(SizeToContentProperty) ?? SizeToContent.Manual);
		set => SetValue(SizeToContentProperty, value);
	}

	// ── Min/Max on a SizeToContent axis ───────────────────────────────────────

	/// <summary>
	/// WPF sizes a SizeToContent axis to the content's desired size, then clamps it to Min/Max. GTK has no such
	/// clamp, so before the first present each fit axis is measured and, when the clamp changes it, given that
	/// default size. A fixed-size child keeps GTK's minimum above a cap (the window cannot shrink below it), so
	/// a capped axis whose content minimum exceeds the cap is hosted in a scrolled window that does not propagate
	/// the minimum there; the content is then clipped, as in WPF.
	/// </summary>
	void ApplySizeToContentLimits(Gtk.Window win)
	{
		var stc = SizeToContent;
		bool fitW = stc == SizeToContent.Width || stc == SizeToContent.WidthAndHeight;
		bool fitH = stc == SizeToContent.Height || stc == SizeToContent.WidthAndHeight;
		if (!fitW && !fitH)
			return;
		bool capW = fitW && !double.IsPositiveInfinity(MaxWidth);
		bool capH = fitH && !double.IsPositiveInfinity(MaxHeight);
		bool raiseW = fitW && MinWidth > 0;
		bool raiseH = fitH && MinHeight > 0;
		if (!capW && !capH && !raiseW && !raiseH)
			return;

		win.Measure(Gtk.Orientation.Horizontal, -1, out int minW, out int natW, out _, out _);
		int targetW = fitW ? Clamp(natW, MinWidth, MaxWidth) : -1;
		win.Measure(Gtk.Orientation.Vertical, fitW ? targetW : -1, out int minH, out int natH, out _, out _);
		int targetH = fitH ? Clamp(natH, MinHeight, MaxHeight) : -1;

		bool clipW = capW && minW > targetW;
		bool clipH = capH && minH > targetH;
		if (clipW || clipH)
			HostInClip(win, clipW, clipH);

		bool changeW = fitW && targetW != natW;
		bool changeH = fitH && targetH != natH;
		if (changeW || changeH)
			win.SetDefaultSize(changeW ? targetW : -1, changeH ? targetH : -1);
	}

	static int Clamp(int natural, double min, double max)
	{
		double value = natural;
		if (!double.IsNaN(max)) value = Math.Min(value, max);
		if (!double.IsNaN(min)) value = Math.Max(value, min);
		return (int)Math.Round(value);
	}

	void HostInClip(Gtk.Window win, bool clipWidth, bool clipHeight)
	{
		if (_contentOverlay == null || ReferenceEquals(win.Child, _clipHost))
			return;
		var host = new Gtk.ScrolledWindow
		{
			HscrollbarPolicy = clipWidth ? Gtk.PolicyType.External : Gtk.PolicyType.Never,
			VscrollbarPolicy = clipHeight ? Gtk.PolicyType.External : Gtk.PolicyType.Never,
			PropagateNaturalWidth = true,
			PropagateNaturalHeight = true,
		};
		win.Child = null;
		host.Child = _contentOverlay;
		win.Child = host;
		_clipHost = host;
	}

	Gtk.ScrolledWindow? _clipHost;
}
