using System;

namespace System.Windows.Controls;

public partial class DockPanel
{
	/// <summary>
	/// WPF DockPanel measure: process children sequentially, docking each to a side
	/// and subtracting its size from the remaining slot.
	/// </summary>
	protected override Size MeasureOverride(Size availableSize)
	{
		double usedW = 0, usedH = 0;
		double maxW = 0, maxH = 0;
		int count = Children.Count;

		for (int i = 0; i < count; i++)
		{
			var child = Children[i];
			bool isFill = LastChildFill && i == count - 1;

			double childAvailW = Math.Max(0, availableSize.Width - usedW);
			double childAvailH = Math.Max(0, availableSize.Height - usedH);

			child.Measure(new Size(childAvailW, childAvailH));

			var dock = isFill ? Dock.Left : GetDock(child); // fill child gets remaining

			if (isFill)
			{
				maxW = Math.Max(maxW, usedW + child.DesiredSize.Width);
				maxH = Math.Max(maxH, usedH + child.DesiredSize.Height);
			}
			else if (dock == Dock.Left || dock == Dock.Right)
			{
				usedW += child.DesiredSize.Width;
				maxH = Math.Max(maxH, usedH + child.DesiredSize.Height);
			}
			else // Top / Bottom
			{
				usedH += child.DesiredSize.Height;
				maxW = Math.Max(maxW, usedW + child.DesiredSize.Width);
			}
		}

		return new Size(Math.Max(maxW, usedW), Math.Max(maxH, usedH));
	}

	/// <summary>WPF DockPanel arrange: dock each child, shrink remaining slot.</summary>
	protected override Size ArrangeOverride(Size finalSize)
	{
		double left = 0, top = 0;
		double right = finalSize.Width, bottom = finalSize.Height;
		int count = Children.Count;

		for (int i = 0; i < count; i++)
		{
			var child = Children[i];
			bool isFill = LastChildFill && i == count - 1;
			Rect slot;

			if (isFill)
			{
				slot = new Rect(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
			}
			else
			{
				var dock = GetDock(child);
				switch (dock)
				{
					case Dock.Left:
						slot = new Rect(left, top, child.DesiredSize.Width, Math.Max(0, bottom - top));
						left += child.DesiredSize.Width;
						break;
					case Dock.Right:
						slot = new Rect(Math.Max(left, right - child.DesiredSize.Width), top,
							child.DesiredSize.Width, Math.Max(0, bottom - top));
						right -= child.DesiredSize.Width;
						break;
					case Dock.Top:
						slot = new Rect(left, top, Math.Max(0, right - left), child.DesiredSize.Height);
						top += child.DesiredSize.Height;
						break;
					default: // Bottom
						slot = new Rect(left, Math.Max(top, bottom - child.DesiredSize.Height),
							Math.Max(0, right - left), child.DesiredSize.Height);
						bottom -= child.DesiredSize.Height;
						break;
				}
			}

			child.Arrange(slot);
		}

		return finalSize;
	}
}