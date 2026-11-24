namespace System.Windows.Controls;

public partial class ScrollViewer
{
	/// <summary>
	/// WPF ScrollViewer.MeasureOverride: the content is measured unconstrained on every axis that can scroll
	/// (visibility other than Disabled) and keeps the constraint on a Disabled axis. The desired size is the
	/// content's, clamped to the constraint, because scrolling absorbs the excess.
	/// </summary>
	protected override Size MeasureOverride(Size constraint)
	{
		if (Content is not UIElement child)
			return new Size(0, 0);
		var offered = new Size(
			HorizontalScrollBarVisibility == ScrollBarVisibility.Disabled ? constraint.Width : double.PositiveInfinity,
			VerticalScrollBarVisibility == ScrollBarVisibility.Disabled ? constraint.Height : double.PositiveInfinity);
		child.Measure(offered);
		return new Size(
			Math.Min(child.DesiredSize.Width, constraint.Width),
			Math.Min(child.DesiredSize.Height, constraint.Height));
	}

	// Gtk.ScrolledWindow reports its child's natural size only when asked to; without it a ScrollViewer is about
	// 0 px for its parent and for SizeToContent. A Disabled axis (PolicyType.Never) already propagates the minimum.
	void ApplyNaturalSizePropagation()
	{
		_scroll.PropagateNaturalWidth = true;
		_scroll.PropagateNaturalHeight = true;
	}
}
