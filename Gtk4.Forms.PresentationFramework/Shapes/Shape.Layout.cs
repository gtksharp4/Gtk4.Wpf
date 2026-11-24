using System;
using System.Windows.Media;

namespace System.Windows.Shapes;

// WPF Shape layout: natural size, stretch matrix, measure/arrange (dotnet/wpf Shape.cs).
public abstract partial class Shape
{
	Matrix _stretchMatrix = Matrix.Identity;

	/// <summary>Maps geometry coordinates to the arranged size; identity for <see cref="Stretch.None"/>.</summary>
	internal Matrix StretchMatrix => _stretchMatrix;

	/// <summary>The geometry that defines this shape in its own coordinates; null when it has none.</summary>
	internal virtual Geometry? GetDefiningGeometry() => null;

	/// <summary>Whether the path is scaled by the stretch matrix (Rectangle/Ellipse size their rect instead).</summary>
	internal virtual bool UsesStretchMatrix => true;

	internal void RedrawShape() => QueueRedraw();

	static void MeasureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((Shape)d).OnMeasureAffected();

	/// <summary>Invalidates layout, resizes the GTK content request and repaints.</summary>
	internal void OnMeasureAffected()
	{
		InvalidateMeasure();
		SyncNaturalSize();
		QueueRedraw();
	}

	/// <summary>0 when there is no stroke or the thickness is NaN, otherwise |StrokeThickness|.</summary>
	internal double GetStrokeThickness()
	{
		double thickness = StrokeThickness;
		return Stroke == null || double.IsNaN(thickness) ? 0 : Math.Abs(thickness);
	}

	/// <summary>The stroke pen without dashes, or null when nothing is stroked.</summary>
	internal Pen? GetPen()
	{
		double thickness = GetStrokeThickness();
		if (Stroke == null || thickness == 0) return null;
		return new Pen(Stroke, thickness)
		{
			StartLineCap = StrokeStartLineCap,
			EndLineCap = StrokeEndLineCap,
			DashCap = StrokeDashCap,
			LineJoin = StrokeLineJoin,
			MiterLimit = StrokeMiterLimit,
		};
	}

	/// <summary>The pen-inflated far corner of the defining geometry, measured from the origin.</summary>
	internal virtual Size GetNaturalSize()
	{
		var geometry = GetDefiningGeometry();
		if (geometry == null) return new Size(0, 0);
		var bounds = geometry.GetRenderBounds(GetPen());
		return new Size(Math.Max(bounds.Right, 0), Math.Max(bounds.Bottom, 0));
	}

	internal static bool SizeIsInvalidOrEmpty(Size size) =>
		double.IsNaN(size.Width) || double.IsNaN(size.Height) || size.IsEmpty;

	/// <inheritdoc/>
	protected override Size MeasureOverride(Size constraint)
	{
		Size size;
		var geometry = GetDefiningGeometry();
		bool infiniteWidth = double.IsInfinity(constraint.Width);
		bool infiniteHeight = double.IsInfinity(constraint.Height);
		if (geometry == null)
			size = new Size(0, 0);
		else if (Stretch == Stretch.None || (Stretch == Stretch.UniformToFill && infiniteWidth && infiniteHeight))
			size = GetNaturalSize();
		else if (Stretch == Stretch.UniformToFill && (infiniteWidth || infiniteHeight))
		{
			double side = Math.Min(constraint.Width, constraint.Height);
			size = new Size(side, side);
		}
		else
			size = GetStretchedRenderSize(Stretch, GetStrokeThickness(), constraint, geometry.GetTransformedBounds());
		return SizeIsInvalidOrEmpty(size) ? new Size(0, 0) : size;
	}

	/// <inheritdoc/>
	protected override Size ArrangeOverride(Size finalSize)
	{
		UpdateStretchMatrix(finalSize);
		return finalSize;
	}

	/// <summary>Recomputes <see cref="StretchMatrix"/> for an arranged (or painted) size.</summary>
	internal void UpdateStretchMatrix(Size size)
	{
		var geometry = GetDefiningGeometry();
		if (Stretch == Stretch.None || !UsesStretchMatrix || geometry == null)
		{
			_stretchMatrix = Matrix.Identity;
			return;
		}
		GetStretchedRenderSizeAndSetStretchMatrix(
			Stretch, GetStrokeThickness(), size, geometry.GetTransformedBounds(), out _stretchMatrix);
	}

	/// <summary>Builds the render path with the stretch matrix applied to the geometry only (not the pen).</summary>
	void BuildStretchedPath(Cairo.Context cr, int width, int height)
	{
		UpdateStretchMatrix(new Size(width, height));
		var m = _stretchMatrix;
		if (m.IsIdentity)
		{
			BuildShapePath(cr, width, height);
			return;
		}
		var saved = cr.Matrix;
		cr.Transform(new Cairo.Matrix(m.M11, m.M12, m.M21, m.M22, m.OffsetX, m.OffsetY));
		BuildShapePath(cr, width, height);
		cr.Matrix = saved;
	}

	/// <summary>Sets the drawing area's content request: the natural size for Stretch.None, else none.</summary>
	internal void SyncNaturalSize()
	{
		if (_area == null) return;
		var size = Stretch == Stretch.None ? GetNaturalSize() : new Size(0, 0);
		_area.ContentWidth = ToRequest(size.Width);
		_area.ContentHeight = ToRequest(size.Height);
	}

	static int ToRequest(double value) =>
		double.IsNaN(value) || value <= 0 ? 0 : (int)Math.Min(Math.Ceiling(value - 1e-6), 100000);
}
