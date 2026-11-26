using System;

namespace System.Windows;

public abstract partial class FrameworkElement
{
	/// <summary>
	/// Pushes the WPF layout properties onto the backing GTK widget once it is created.
	/// WPF's measure/arrange computes desired sizes, but GTK performs the real on-screen
	/// layout, so <see cref="Width"/>/<see cref="Height"/> and
	/// <see cref="HorizontalAlignment"/>/<see cref="VerticalAlignment"/> must be mapped to the
	/// widget's size request and GTK alignment, or they are silently ignored.
	/// </summary>
	private void ApplyLayoutToWidget(Gtk.Widget widget)
	{
		ApplyExplicitSize(widget);

		var ha = HorizontalAlignment;
		var va = VerticalAlignment;

		// WPF: an explicit size on an otherwise-Stretch axis pins the element to that size,
		// centered within its slot (Stretch no longer fills). Mirror that so a Width/Height
		// does not stretch to fill the parent.
		if (!double.IsNaN(Width) && ha == HorizontalAlignment.Stretch)
			ha = HorizontalAlignment.Center;
		if (!double.IsNaN(Height) && va == VerticalAlignment.Stretch)
			va = VerticalAlignment.Center;

		widget.Halign = ToGtkAlign(ha);
		widget.Valign = ToGtkAlign(va);
	}

	/// <summary>
	/// Maps explicit <see cref="Width"/>/<see cref="Height"/> (NaN = auto) to the widget's
	/// size request. <see cref="Window"/> overrides this to set the top-level default size
	/// instead — a window's size is its initial extent, not a minimum-size request.
	/// </summary>
	protected virtual void ApplyExplicitSize(Gtk.Widget widget)
	{
		int w = double.IsNaN(Width) ? -1 : (int)Math.Round(Width);
		int h = double.IsNaN(Height) ? -1 : (int)Math.Round(Height);
		if (w >= 0 || h >= 0)
			widget.SetSizeRequest(w, h);
	}

	static Gtk.Align ToGtkAlign(HorizontalAlignment a) => a switch
	{
		HorizontalAlignment.Left => Gtk.Align.Start,
		HorizontalAlignment.Center => Gtk.Align.Center,
		HorizontalAlignment.Right => Gtk.Align.End,
		_ => Gtk.Align.Fill,
	};

	static Gtk.Align ToGtkAlign(VerticalAlignment a) => a switch
	{
		VerticalAlignment.Top => Gtk.Align.Start,
		VerticalAlignment.Center => Gtk.Align.Center,
		VerticalAlignment.Bottom => Gtk.Align.End,
		_ => Gtk.Align.Fill,
	};
}