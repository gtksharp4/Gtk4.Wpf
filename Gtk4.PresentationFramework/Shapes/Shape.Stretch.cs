using System;
using System.Windows.Media;

namespace System.Windows.Shapes;

// Pure stretch arithmetic (dotnet/wpf Shape.GetStretchMetrics).
public abstract partial class Shape
{
	internal static Size GetStretchedRenderSize(
		Stretch mode, double strokeThickness, Size availableSize, Rect geometryBounds)
	{
		GetStretchMetrics(mode, strokeThickness, availableSize, geometryBounds,
			out _, out _, out _, out _, out var size);
		return size;
	}

	internal static Size GetStretchedRenderSizeAndSetStretchMatrix(
		Stretch mode, double strokeThickness, Size availableSize, Rect geometryBounds, out Matrix stretchMatrix)
	{
		GetStretchMetrics(mode, strokeThickness, availableSize, geometryBounds,
			out double xScale, out double yScale, out double dX, out double dY, out var size);
		stretchMatrix = new Matrix(xScale, 0, 0, yScale, dX, dY);
		return size;
	}

	internal static void GetStretchMetrics(
		Stretch mode, double strokeThickness, Size availableSize, Rect bounds,
		out double xScale, out double yScale, out double dX, out double dY, out Size stretchedSize)
	{
		if (bounds.IsEmpty)
		{
			xScale = yScale = 1;
			dX = dY = 0;
			stretchedSize = new Size(0, 0);
			return;
		}
		double margin = strokeThickness / 2;
		bool thinDimension = false;
		xScale = Math.Max(availableSize.Width - strokeThickness, 0);
		yScale = Math.Max(availableSize.Height - strokeThickness, 0);

		if (bounds.Width > xScale * float.Epsilon)
			xScale /= bounds.Width;
		else
		{
			xScale = 1;
			if (bounds.Width == 0) thinDimension = true;
		}
		if (bounds.Height > yScale * float.Epsilon)
			yScale /= bounds.Height;
		else
		{
			yScale = 1;
			if (bounds.Height == 0) thinDimension = true;
		}

		if (mode != Stretch.Fill && !thinDimension)
		{
			if (mode == Stretch.Uniform)
			{
				if (xScale > yScale) xScale = yScale; else yScale = xScale;
			}
			else if (xScale < yScale) xScale = yScale; else yScale = xScale;
		}

		dX = margin - bounds.Left * xScale;
		dY = margin - bounds.Top * yScale;
		stretchedSize = new Size(bounds.Width * xScale + strokeThickness, bounds.Height * yScale + strokeThickness);
	}
}
