namespace System.Windows.Controls;

public partial class ScrollViewer
{
	/// <summary>
	/// WPF ScrollViewer.MeasureOverride: the template adds Padding, BorderThickness and a Visible scroll bar
	/// around the content. The content is measured unconstrained on every axis that can scroll (visibility other
	/// than Disabled) and gets the constraint minus that chrome on a Disabled axis. The desired size is the
	/// content plus the chrome, clamped to the constraint, because scrolling absorbs the excess.
	/// </summary>
	protected override Size MeasureOverride(Size constraint)
	{
		var pad = Padding;
		var border = BorderThickness;
		double extraW = pad.Left + pad.Right + border.Left + border.Right
			+ (VerticalScrollBarVisibility == ScrollBarVisibility.Visible ? SystemParameters.VerticalScrollBarWidth : 0);
		double extraH = pad.Top + pad.Bottom + border.Top + border.Bottom
			+ (HorizontalScrollBarVisibility == ScrollBarVisibility.Visible
				? SystemParameters.HorizontalScrollBarHeight : 0);
		var content = new Size(0, 0);
		if (Content is UIElement child)
		{
			child.Measure(new Size(
				HorizontalScrollBarVisibility == ScrollBarVisibility.Disabled
					? Math.Max(0, constraint.Width - extraW) : double.PositiveInfinity,
				VerticalScrollBarVisibility == ScrollBarVisibility.Disabled
					? Math.Max(0, constraint.Height - extraH) : double.PositiveInfinity));
			content = child.DesiredSize;
		}
		return new Size(
			Math.Min(content.Width + extraW, constraint.Width),
			Math.Min(content.Height + extraH, constraint.Height));
	}

	// Gtk.ScrolledWindow reports its child's natural size only when asked to; without it a ScrollViewer is about
	// 0 px for its parent and for SizeToContent. It is only asked on an axis where the widget does not expand:
	// Gtk.Grid hands every child its natural size before sharing the expand space, so a propagating ScrollViewer
	// in a star track would take the whole track (WPF splits star tracks by weight, whatever the child wants).
	void ApplyNaturalSizePropagation()
	{
		_scroll.AddNotification("hexpand", (_, _) => UpdateNaturalSizePropagation());
		_scroll.AddNotification("vexpand", (_, _) => UpdateNaturalSizePropagation());
		UpdateNaturalSizePropagation();
	}

	void UpdateNaturalSizePropagation()
	{
		_scroll.PropagateNaturalWidth = !_scroll.Hexpand;
		_scroll.PropagateNaturalHeight = !_scroll.Vexpand;
	}
}
