namespace System.Windows.Controls;

public partial class ScrollViewer
{
	/// <summary>
	/// WPF ScrollViewer.MeasureOverride: the default template adds Padding (the ScrollContentPresenter margin)
	/// and a visible scroll bar around the content; it has no Border, so BorderThickness adds nothing. The content
	/// is measured unconstrained on every axis that can scroll and gets the constraint minus that chrome on a
	/// Disabled axis. An Auto bar becomes visible when the extent exceeds the viewport, and its size is added.
	/// The desired size is the content plus the chrome, clamped to the constraint.
	/// </summary>
	protected override Size MeasureOverride(Size constraint)
	{
		var pad = Padding;
		var vBar = VerticalScrollBarVisibility;
		var hBar = HorizontalScrollBarVisibility;
		bool showV = vBar == ScrollBarVisibility.Visible;
		bool showH = hBar == ScrollBarVisibility.Visible;
		var content = new Size(0, 0);
		double extraW = 0, extraH = 0;
		for (int pass = 0; pass < 3; pass++)
		{
			extraW = pad.Left + pad.Right + (showV ? SystemParameters.VerticalScrollBarWidth : 0);
			extraH = pad.Top + pad.Bottom + (showH ? SystemParameters.HorizontalScrollBarHeight : 0);
			if (Content is UIElement child)
			{
				child.Measure(new Size(
					hBar == ScrollBarVisibility.Disabled
						? Math.Max(0, constraint.Width - extraW) : double.PositiveInfinity,
					vBar == ScrollBarVisibility.Disabled
						? Math.Max(0, constraint.Height - extraH) : double.PositiveInfinity));
				content = child.DesiredSize;
			}
			bool addV = !showV && vBar == ScrollBarVisibility.Auto && content.Height + extraH > constraint.Height;
			bool addH = !showH && hBar == ScrollBarVisibility.Auto && content.Width + extraW > constraint.Width;
			if (!addV && !addH)
				break;
			showV |= addV;
			showH |= addH;
		}
		return new Size(
			Math.Min(content.Width + extraW, constraint.Width),
			Math.Min(content.Height + extraH, constraint.Height));
	}
}
