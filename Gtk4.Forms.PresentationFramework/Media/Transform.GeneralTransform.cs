namespace System.Windows.Media;

public abstract partial class Transform
{
	/// <inheritdoc/>
	public override GeneralTransform? Inverse
	{
		get
		{
			Matrix m = Value;
			if (!m.HasInverse) return null;
			m.Invert();
			return new MatrixTransform(m);
		}
	}

	/// <inheritdoc/>
	public override Transform? AffineTransform => this;

	/// <inheritdoc/>
	public override bool TryTransform(Point inPoint, out Point result)
	{
		result = Value.Transform(inPoint);
		return true;
	}

	/// <inheritdoc/>
	public override Rect TransformBounds(Rect rect) => BoundsOf(Value, rect);

	internal static Rect BoundsOf(Matrix m, Rect rect)
	{
		if (rect.IsEmpty) return rect;
		Point a = m.Transform(new Point(rect.X, rect.Y));
		Point b = m.Transform(new Point(rect.X + rect.Width, rect.Y));
		Point c = m.Transform(new Point(rect.X, rect.Y + rect.Height));
		Point d = m.Transform(new Point(rect.X + rect.Width, rect.Y + rect.Height));
		double x0 = Math.Min(Math.Min(a.X, b.X), Math.Min(c.X, d.X));
		double y0 = Math.Min(Math.Min(a.Y, b.Y), Math.Min(c.Y, d.Y));
		double x1 = Math.Max(Math.Max(a.X, b.X), Math.Max(c.X, d.X));
		double y1 = Math.Max(Math.Max(a.Y, b.Y), Math.Max(c.Y, d.Y));
		return new Rect(x0, y0, x1 - x0, y1 - y0);
	}
}
