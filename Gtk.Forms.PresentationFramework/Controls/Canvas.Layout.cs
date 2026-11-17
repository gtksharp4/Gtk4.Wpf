using System;

namespace System.Windows.Controls;

public partial class Canvas
{
	/// <summary>
	/// WPF Canvas measure: measure each child with infinite available size;
	/// desired size is always (0,0) — Canvas never influences its own size.
	/// </summary>
	protected override Size MeasureOverride(Size availableSize)
	{
		foreach (UIElement child in Children)
			child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		return new Size(0, 0);
	}

	/// <summary>
	/// WPF Canvas arrange: place each child at Canvas.Left/Top (falling back to 0).
	/// Canvas.Right/Bottom are resolved relative to the final size.
	/// </summary>
	protected override Size ArrangeOverride(Size finalSize)
	{
		foreach (UIElement child in Children)
		{
			double x = ResolveX(child, finalSize.Width);
			double y = ResolveY(child, finalSize.Height);
			child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));
		}
		return finalSize;
	}

	static double ResolveX(UIElement child, double canvasWidth)
	{
		double left = GetLeft(child);
		if (!double.IsNaN(left)) return left;
		double right = GetRight(child);
		if (!double.IsNaN(right)) return canvasWidth - child.DesiredSize.Width - right;
		return 0;
	}

	static double ResolveY(UIElement child, double canvasHeight)
	{
		double top = GetTop(child);
		if (!double.IsNaN(top)) return top;
		double bottom = GetBottom(child);
		if (!double.IsNaN(bottom)) return canvasHeight - child.DesiredSize.Height - bottom;
		return 0;
	}
}