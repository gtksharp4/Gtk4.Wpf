using System;
using System.Collections.Generic;

namespace System.Windows.Controls;

public partial class WrapPanel
{
	/// <summary>
	/// WPF WrapPanel measure: flow children, wrapping when the main-axis limit is exceeded.
	/// </summary>
	protected override Size MeasureOverride(Size availableSize)
	{
		bool isHorizontal = Orientation == Orientation.Horizontal;
		double mainLimit = isHorizontal ? availableSize.Width : availableSize.Height;
		if (double.IsPositiveInfinity(mainLimit)) mainLimit = double.MaxValue;

		double lineMain = 0, lineCross = 0;
		double totalMain = 0, totalCross = 0;

		foreach (UIElement child in Children)
		{
			child.Measure(ItemConstraint());
			double childMain = isHorizontal ? ItemWidthOf(child) : ItemHeightOf(child);
			double childCross = isHorizontal ? ItemHeightOf(child) : ItemWidthOf(child);

			if (lineMain > 0 && lineMain + childMain > mainLimit)
			{
				// Wrap: start a new line.
				totalMain = Math.Max(totalMain, lineMain);
				totalCross += lineCross;
				lineMain = 0;
				lineCross = 0;
			}

			lineMain += childMain;
			lineCross = Math.Max(lineCross, childCross);
		}

		totalMain = Math.Max(totalMain, lineMain);
		totalCross += lineCross;

		return isHorizontal
			? new Size(totalMain, totalCross)
			: new Size(totalCross, totalMain);
	}

	/// <summary>WPF WrapPanel arrange: place children in lines, wrapping as needed.</summary>
	protected override Size ArrangeOverride(Size finalSize)
	{
		bool isHorizontal = Orientation == Orientation.Horizontal;
		double mainLimit = isHorizontal ? finalSize.Width : finalSize.Height;

		// Build lines.
		var lines = new List<(List<UIElement> items, double lineMain, double lineCross)>();
		var currentItems = new List<UIElement>();
		double currentMain = 0, currentCross = 0;

		foreach (UIElement child in Children)
		{
			double childMain = isHorizontal ? ItemWidthOf(child) : ItemHeightOf(child);
			double childCross = isHorizontal ? ItemHeightOf(child) : ItemWidthOf(child);

			if (currentItems.Count > 0 && currentMain + childMain > mainLimit)
			{
				lines.Add((currentItems, currentMain, currentCross));
				currentItems = new List<UIElement>();
				currentMain = 0;
				currentCross = 0;
			}

			currentItems.Add(child);
			currentMain += childMain;
			currentCross = Math.Max(currentCross, childCross);
		}

		if (currentItems.Count > 0)
			lines.Add((currentItems, currentMain, currentCross));

		double crossOffset = 0;
		foreach (var (items, lineMain, lineCross) in lines)
		{
			double mainOffset = 0;
			foreach (var child in items)
			{
				double childMain = isHorizontal ? ItemWidthOf(child) : ItemHeightOf(child);
				Rect slot = isHorizontal
					? new Rect(mainOffset, crossOffset, childMain, lineCross)
					: new Rect(crossOffset, mainOffset, lineCross, childMain);
				child.Arrange(slot);
				mainOffset += childMain;
			}
			crossOffset += lineCross;
		}

		return finalSize;
	}
}