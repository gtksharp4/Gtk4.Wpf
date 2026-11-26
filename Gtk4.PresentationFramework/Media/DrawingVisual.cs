namespace System.Windows.Media;

/// <summary>
/// A visual that supports direct drawing via <see cref="RenderOpen"/>. Mirrors WPF's DrawingVisual.
/// The recorded commands are replayed through Cairo by the hosting element's render layer.
/// </summary>
public class DrawingVisual : ContainerVisual
{
	DrawingContext? _drawing;

	/// <summary>The last <see cref="DrawingContext"/> committed to this visual.</summary>
	public DrawingContext? Drawing => _drawing;

	/// <summary>
	/// Opens a context for recording; <see cref="DrawingContext.Close"/> or Dispose commits it, replacing
	/// any previous recording.
	/// </summary>
	public DrawingContext RenderOpen() => new DrawingContext(CommitDrawing);

	internal void CommitDrawing(DrawingContext ctx)
	{
		_drawing = ctx;
		InvalidateRender();
	}

	internal override Rect GetContentBounds() =>
		_drawing == null ? Rect.Empty : DrawingContextRenderer.ComputeBounds(_drawing, includeText: true);

	/// <inheritdoc/>
	protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
	{
		Rect b = GetContentBounds();
		Point p = hitTestParameters.HitPoint;
		return !b.IsEmpty && p.X >= b.X && p.X < b.X + b.Width && p.Y >= b.Y && p.Y < b.Y + b.Height
			? new PointHitTestResult(this, p)
			: null;
	}

	/// <inheritdoc/>
	protected override GeometryHitTestResult? HitTestCore(GeometryHitTestParameters hitTestParameters)
	{
		Rect b = GetContentBounds();
		Rect g = hitTestParameters.HitGeometry.Bounds;
		if (b.IsEmpty || g.IsEmpty) return null;
		bool apart = g.X >= b.X + b.Width || g.X + g.Width <= b.X || g.Y >= b.Y + b.Height || g.Y + g.Height <= b.Y;
		if (apart) return null;
		bool inside = g.X >= b.X && g.Y >= b.Y && g.X + g.Width <= b.X + b.Width && g.Y + g.Height <= b.Y + b.Height;
		bool contains = b.X >= g.X && b.Y >= g.Y && b.X + b.Width <= g.X + g.Width && b.Y + b.Height <= g.Y + g.Height;
		return new GeometryHitTestResult(this, inside ? IntersectionDetail.FullyInside
			: contains ? IntersectionDetail.FullyContains : IntersectionDetail.Intersects);
	}
}
