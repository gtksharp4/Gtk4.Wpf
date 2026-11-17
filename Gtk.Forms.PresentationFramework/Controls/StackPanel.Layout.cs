using System;

namespace System.Windows.Controls;

public partial class StackPanel
{
	/// <summary>
	/// WPF StackPanel measure: sum children along orientation; max across.
	/// Children get infinite constraint in the stacking direction.
	/// </summary>
	protected override Size MeasureOverride(Size availableSize)
	{
		bool isVertical = Orientation == Orientation.Vertical;
		double totalMain = 0;
		double maxCross = 0;

		foreach (UIElement child in Children)
		{
			var constraint = isVertical
				? new Size(availableSize.Width, double.PositiveInfinity)
				: new Size(double.PositiveInfinity, availableSize.Height);
			child.Measure(constraint);
			if (isVertical)
			{
				totalMain += child.DesiredSize.Height;
				maxCross = Math.Max(maxCross, child.DesiredSize.Width);
			}
			else
			{
				totalMain += child.DesiredSize.Width;
				maxCross = Math.Max(maxCross, child.DesiredSize.Height);
			}
		}

		return isVertical
			? new Size(maxCross, totalMain)
			: new Size(totalMain, maxCross);
	}

	/// <summary>
	/// WPF StackPanel arrange: stack children sequentially along orientation.
	/// </summary>
	protected override Size ArrangeOverride(Size finalSize)
	{
		bool isVertical = Orientation == Orientation.Vertical;
		double offset = 0;

		foreach (UIElement child in Children)
		{
			Rect slot;
			if (isVertical)
			{
				slot = new Rect(0, offset, finalSize.Width, child.DesiredSize.Height);
				offset += child.DesiredSize.Height;
			}
			else
			{
				slot = new Rect(offset, 0, child.DesiredSize.Width, finalSize.Height);
				offset += child.DesiredSize.Width;
			}
			child.Arrange(slot);
		}

		return finalSize;
	}
}