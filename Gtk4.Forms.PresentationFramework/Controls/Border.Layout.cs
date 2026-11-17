using System;

namespace System.Windows.Controls;

public partial class Border
{
	/// <summary>
	/// WPF Border measure: deflate by Padding+BorderThickness, measure child,
	/// inflate back.
	/// </summary>
	protected override Size MeasureOverride(Size availableSize)
	{
		var padding = Padding;
		var border = BorderThickness;
		double insetW = padding.Left + padding.Right + border.Left + border.Right;
		double insetH = padding.Top + padding.Bottom + border.Top + border.Bottom;

		double innerW = Math.Max(0, availableSize.Width - insetW);
		double innerH = Math.Max(0, availableSize.Height - insetH);

		if (Child != null)
		{
			Child.Measure(new Size(innerW, innerH));
			return new Size(Child.DesiredSize.Width + insetW, Child.DesiredSize.Height + insetH);
		}

		return new Size(insetW, insetH);
	}

	/// <summary>WPF Border arrange: inset child by Padding+BorderThickness.</summary>
	protected override Size ArrangeOverride(Size finalSize)
	{
		if (Child == null) return finalSize;

		var padding = Padding;
		var border = BorderThickness;
		double insetL = padding.Left + border.Left;
		double insetT = padding.Top + border.Top;
		double insetW = insetL + padding.Right + border.Right;
		double insetH = insetT + padding.Bottom + border.Bottom;

		double childW = Math.Max(0, finalSize.Width - insetW);
		double childH = Math.Max(0, finalSize.Height - insetH);

		Child.Arrange(new Rect(insetL, insetT, childW, childH));
		return finalSize;
	}
}