using System;

namespace System.Windows.Controls.Primitives;

public partial class UniformGrid
{
	/// <summary>
	/// WPF UniformGrid measure: cells are equal-sized; each child gets 1/cols × 1/rows.
	/// </summary>
	protected override Size MeasureOverride(Size availableSize)
	{
		var (rows, cols) = ComputeGrid(Rows, Columns, Children.Count);

		double cellW = double.IsPositiveInfinity(availableSize.Width) ? double.PositiveInfinity : availableSize.Width / cols;
		double cellH = double.IsPositiveInfinity(availableSize.Height) ? double.PositiveInfinity : availableSize.Height / rows;
		var cellSize = new Size(cellW, cellH);

		double maxChildW = 0, maxChildH = 0;
		foreach (UIElement child in Children)
		{
			child.Measure(cellSize);
			maxChildW = Math.Max(maxChildW, child.DesiredSize.Width);
			maxChildH = Math.Max(maxChildH, child.DesiredSize.Height);
		}

		return new Size(maxChildW * cols, maxChildH * rows);
	}

	/// <summary>WPF UniformGrid arrange: place each child in its equal cell.</summary>
	protected override Size ArrangeOverride(Size finalSize)
	{
		var (rows, cols) = ComputeGrid(Rows, Columns, Children.Count);

		double cellW = finalSize.Width / cols;
		double cellH = finalSize.Height / rows;

		for (int i = 0; i < Children.Count; i++)
		{
			int col = i % cols;
			int row = i / cols;
			Children[i].Arrange(new Rect(col * cellW, row * cellH, cellW, cellH));
		}

		return finalSize;
	}
}