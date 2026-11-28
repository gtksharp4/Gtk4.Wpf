using System;
using System.Windows.Controls;
using System.Windows.Media;

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
	/// affected dimension(s) start at the content size (clamped to Min/Max; sized from the managed Measure
	/// around a ScrollViewer),
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
		var contentElement = Content as UIElement;
		bool managed = HasScrollViewer(contentElement);
		if (!capW && !capH && !raiseW && !raiseH && !managed)
			return;

		if (managed)
		{
			contentElement!.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			RequestScrollViewerExtents(contentElement, fitW && !capW, fitH && !capH);
		}
		win.Measure(Gtk.Orientation.Horizontal, -1, out int minW, out int natW, out _, out _);
		var desired = managed ? ManagedExtraSize(contentElement!) : default;
		if (fitW) natW += (int)Math.Ceiling(desired.Width);
		int targetW = fitW ? Clamp(natW, MinWidth, WorkLimit(MaxWidth, managed, SystemParameters.WorkArea.Width)) : -1;
		win.Measure(Gtk.Orientation.Vertical, fitW ? targetW : -1, out int minH, out int natH, out _, out _);
		if (fitH) natH += (int)Math.Ceiling(desired.Height);
		int targetH = fitH ? Clamp(natH, MinHeight, WorkLimit(MaxHeight, managed, SystemParameters.WorkArea.Height)) : -1;

		bool clipW = capW && minW > targetW;
		bool clipH = capH && minH > targetH;
		if (clipW || clipH)
			HostInClip(win, clipW, clipH);

		bool changeW = fitW && (targetW != natW || desired.Width > 0);
		bool changeH = fitH && (targetH != natH || desired.Height > 0);
		if (changeW || changeH)
		{
			// -1 would clear the default ApplyWindowSize gave the axis SizeToContent does not size.
			int otherW = AxisSize(false, Width, MinWidth, MaxWidth, SystemParameters.WorkArea.Width, out _);
			int otherH = AxisSize(false, Height, MinHeight, MaxHeight, SystemParameters.WorkArea.Height, out _);
			win.SetDefaultSize(fitW ? (changeW ? targetW : -1) : otherW, fitH ? (changeH ? targetH : -1) : otherH);
		}
	}

	static int Clamp(int natural, double min, double max)
	{
		double value = natural;
		if (!double.IsNaN(max)) value = Math.Min(value, max);
		if (!double.IsNaN(min)) value = Math.Max(value, min);
		return (int)Math.Round(value);
	}

	// The content overlay of a window widget, with or without the explicit-size clip host.
	internal static Gtk.Overlay? ContentOverlayOf(Gtk.Window? win)
	{
		var child = win?.Child;
		if (child is Gtk.ScrolledWindow sw)
			child = sw.Child is Gtk.Viewport vp ? vp.Child : sw.Child;
		return child as Gtk.Overlay;
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

	// WPF never grows a Manual window to its content (the content is clipped), whereas GTK raises a window's
	// minimum to its child's. Every axis SizeToContent does not size (explicit, or NaN and so at the default
	// size) is hosted in a scrolled window that does
	// not propagate the minimum, so a window whose content overflows (a height bound to ActualHeight, a fixed-size
	// child) keeps its size.
	void HostManualAxesInClip(Gtk.Window win)
	{
		var stc = SizeToContent;
		bool manualW = stc != SizeToContent.Width && stc != SizeToContent.WidthAndHeight;
		bool manualH = stc != SizeToContent.Height && stc != SizeToContent.WidthAndHeight;
		if (_clipHost != null)
		{
			if (manualW) _clipHost.HscrollbarPolicy = Gtk.PolicyType.External;
			if (manualH) _clipHost.VscrollbarPolicy = Gtk.PolicyType.External;
			return;
		}
		if (!manualW && !manualH)
			return;
		HostInClip(win, manualW, manualH);
	}

	Gtk.ScrolledWindow? _clipHost;

	// A SizeToContent window grows to the content's WPF desired size, but never past the work area.
	static double WorkLimit(double max, bool managed, double workArea) =>
		managed ? Math.Min(max, workArea) : max;

	// GTK's natural size of a scrolled window is not its content's, so a window around a ScrollViewer is
	// sized from the managed Measure (infinite constraint, as WPF does for SizeToContent).
	static bool HasScrollViewer(UIElement? element)
	{
		if (element == null)
			return false;
		if (element is ScrollViewer)
			return true;
		int count = VisualTreeHelper.GetChildrenCount(element);
		for (int i = 0; i < count; i++)
			if (VisualTreeHelper.GetChild(element, i) is UIElement child && HasScrollViewer(child))
				return true;
		return false;
	}

	// GTK allocates a nested scrolled window its natural size, not the content's, and the window's default size
	// alone does not hand the difference to it (a Box or Grid gives extra space only to expanding children). Each
	// ScrollViewer therefore requests the extent its managed WPF Measure wants, on an axis the window fits and
	// does not cap, unless that extent alone would not fit the work area (then it scrolls, as before). An axis the
	// ScrollViewer sizes explicitly already carries its request.
	static void RequestScrollViewerExtents(UIElement element, bool fitW, bool fitH)
	{
		if (element is ScrollViewer sv)
		{
			var d = sv.DesiredSize;
			int w = fitW && double.IsNaN(sv.Width) && d.Width <= SystemParameters.WorkArea.Width
				? (int)Math.Ceiling(d.Width) : -1;
			int h = fitH && double.IsNaN(sv.Height) && d.Height <= SystemParameters.WorkArea.Height
				? (int)Math.Ceiling(d.Height) : -1;
			if (w >= 0 || h >= 0)
			{
				sv.Widget.GetSizeRequest(out int curW, out int curH);
				sv.Widget.SetSizeRequest(w >= 0 ? w : curW, h >= 0 ? h : curH);
			}
			return;
		}
		int count = VisualTreeHelper.GetChildrenCount(element);
		for (int i = 0; i < count; i++)
			if (VisualTreeHelper.GetChild(element, i) is UIElement child)
				RequestScrollViewerExtents(child, fitW, fitH);
	}

	// How much larger the content's WPF desired size is than GTK's natural size of its widget, per axis.
	static Size ManagedExtraSize(UIElement content)
	{
		content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		content.Widget.Measure(Gtk.Orientation.Horizontal, -1, out _, out int gtkW, out _, out _);
		content.Widget.Measure(Gtk.Orientation.Vertical, -1, out _, out int gtkH, out _, out _);
		return new Size(Math.Max(0, content.DesiredSize.Width - gtkW), Math.Max(0, content.DesiredSize.Height - gtkH));
	}
}
