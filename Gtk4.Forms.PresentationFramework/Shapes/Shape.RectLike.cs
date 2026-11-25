using System;
using System.Windows.Media;

namespace System.Windows.Shapes;

// Rectangle and Ellipse: no defining geometry; the outline is inset by half the stroke (dotnet/wpf Rectangle.cs).
public abstract partial class Shape
{
	/// <summary>Measure for Rectangle/Ellipse: the stroke thickness, or a square from the constraint for UniformToFill.</summary>
	internal Size MeasureRectLike(Size constraint)
	{
		double stroke = GetStrokeThickness();
		var size = new Size(stroke, stroke);
		if (Stretch == Stretch.UniformToFill)
		{
			double w = constraint.Width, h = constraint.Height;
			bool infiniteW = double.IsInfinity(w), infiniteH = double.IsInfinity(h);
			if (!(infiniteW && infiniteH))
			{
				double side = infiniteW || infiniteH ? Math.Min(w, h) : Math.Max(w, h);
				size = new Size(side, side);
			}
		}
		return SizeIsInvalidOrEmpty(size) ? new Size(0, 0) : size;
	}

	/// <summary>The outline rectangle for a painted size; Stretch.None gives an empty (zero-sized) rectangle.</summary>
	internal Rect GetRectLikeRect(double width, double height)
	{
		double stroke = GetStrokeThickness(), half = stroke / 2;
		double w = Math.Max(0, width - stroke), h = Math.Max(0, height - stroke);
		switch (Stretch)
		{
			case Stretch.None:
				w = h = 0;
				break;
			case Stretch.Uniform:
				w = h = Math.Min(w, h);
				break;
			case Stretch.UniformToFill:
				w = h = Math.Max(w, h);
				break;
		}
		return new Rect(half, half, w, h);
	}
}
