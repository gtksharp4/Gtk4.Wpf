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
public abstract partial class FrameworkElement
{
	// WPF clips a child to a fixed slot; GTK lets a child's natural size beat its container's fixed size and squeeze
	// the siblings. The natural request of a Shape, TextBlock or Image is therefore capped by a fixed Grid track (or
	// tracks, for a span) or the explicit size of a parent that sizes its children (not a Border, whose GTK frame
	// adds insets, and not a Canvas, which does not constrain them), and by this element's own Margin. A star or Auto
	// track and an unsized parent do not cap. Read when GTK measures, so a later change shows on the next measure.
	internal double SlotLimit(bool horizontal)
	{
		double limit = double.PositiveInfinity;
		if (Parent is Controls.Grid grid)
		{
			int start = horizontal ? Controls.Grid.GetColumn(this) : Controls.Grid.GetRow(this);
			int span = Math.Max(1, horizontal ? Controls.Grid.GetColumnSpan(this) : Controls.Grid.GetRowSpan(this));
			int count = horizontal ? grid.ColumnDefinitions.Count : grid.RowDefinitions.Count;
			double sum = 0;
			bool fixedTracks = count > 0 && start >= 0 && start < count;
			for (int i = start; fixedTracks && i < start + span && i < count; i++)
			{
				var length = horizontal ? grid.ColumnDefinitions[i].Width : grid.RowDefinitions[i].Height;
				if (horizontal) grid.ColumnDefinitions[i].Owner = grid;
				else grid.RowDefinitions[i].Owner = grid;
				if (length.IsAbsolute) sum += length.Value;
				else fixedTracks = false;
			}
			if (fixedTracks) limit = sum;
		}
		else if (Parent is FrameworkElement parent and not (Controls.Border or Controls.Canvas))
		{
			double size = horizontal ? parent.Width : parent.Height;
			if (!double.IsNaN(size))
				limit = size;
		}
		if (double.IsPositiveInfinity(limit)) return limit;
		var margin = Margin;
		return Math.Max(0, limit - (horizontal ? margin.Left + margin.Right : margin.Top + margin.Bottom));
	}

	/// <summary>Asks GTK to re-measure an element whose natural request is capped by <see cref="SlotLimit"/>.</summary>
	internal virtual void QueueSlotResize() { }

	// A parent's explicit size or a Grid track change caps its children's natural request differently; GTK keeps the
	// cached request, so ask it to re-measure.
	internal static void RefreshSlotCaps(FrameworkElement parent)
	{
		if (parent is Controls.Panel panel)
			foreach (var child in panel.Children)
				(child as FrameworkElement)?.QueueSlotResize();
	}


	/// <summary>
	/// Caps a GTK measure of a Label or Picture at the slot: the minimum goes with it (a clipped element can be
	/// arranged smaller than its content), so the element no longer widens its track or pushes siblings.
	/// </summary>
	internal void CapMeasure(Gtk.Orientation orientation, ref int minimum, ref int natural)
	{
		double limit = SlotLimit(orientation == Gtk.Orientation.Horizontal);
		if (double.IsPositiveInfinity(limit)) return;
		int cap = (int)Math.Max(0, Math.Min(Math.Floor(limit + 1e-6), 100000));
		natural = Math.Min(natural, cap);
		minimum = Math.Min(minimum, natural);
	}
}
