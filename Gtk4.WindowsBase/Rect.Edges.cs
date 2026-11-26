using System;

namespace System.Windows;

public partial struct Rect
{
	/// <summary>Creates a rectangle at the origin with the given size.</summary>
	public Rect(Size size) : this(0, 0, size.Width, size.Height) { }

	/// <summary>Creates the smallest rectangle containing both points.</summary>
	public Rect(Point point1, Point point2)
		: this(Math.Min(point1.X, point2.X), Math.Min(point1.Y, point2.Y),
			Math.Abs(point2.X - point1.X), Math.Abs(point2.Y - point1.Y)) { }

	/// <summary>Creates a rectangle from a corner and a vector to the opposite corner.</summary>
	public Rect(Point point, Vector vector) : this(point, new Point(point.X + vector.X, point.Y + vector.Y)) { }

	/// <summary>The X of the left edge.</summary>
	public double Left => X;

	/// <summary>The Y of the top edge.</summary>
	public double Top => Y;

	/// <summary>The X of the right edge; negative infinity when empty.</summary>
	public double Right => IsEmpty ? double.NegativeInfinity : X + Width;

	/// <summary>The Y of the bottom edge; negative infinity when empty.</summary>
	public double Bottom => IsEmpty ? double.NegativeInfinity : Y + Height;

	/// <summary>The top-left corner.</summary>
	public Point TopLeft => new Point(Left, Top);

	/// <summary>The top-right corner.</summary>
	public Point TopRight => new Point(Right, Top);

	/// <summary>The bottom-left corner.</summary>
	public Point BottomLeft => new Point(Left, Bottom);

	/// <summary>The bottom-right corner.</summary>
	public Point BottomRight => new Point(Right, Bottom);

	/// <summary>Whether <paramref name="point"/> lies inside or on the edge of the rectangle.</summary>
	public bool Contains(Point point) => Contains(point.X, point.Y);

	/// <summary>Whether (<paramref name="x"/>, <paramref name="y"/>) lies inside or on the edge of the rectangle.</summary>
	public bool Contains(double x, double y) =>
		!IsEmpty && x >= X && x - Width <= X && y >= Y && y - Height <= Y;

	/// <summary>Whether <paramref name="rect"/> lies entirely inside this rectangle.</summary>
	public bool Contains(Rect rect) =>
		!IsEmpty && !rect.IsEmpty && X <= rect.X && Y <= rect.Y && X + Width >= rect.X + rect.Width
		&& Y + Height >= rect.Y + rect.Height;

	/// <summary>Whether the two rectangles overlap.</summary>
	public bool IntersectsWith(Rect rect) =>
		!IsEmpty && !rect.IsEmpty && rect.X <= X + Width && rect.X + rect.Width >= X && rect.Y <= Y + Height
		&& rect.Y + rect.Height >= Y;

	/// <summary>Moves the rectangle by the given amounts.</summary>
	/// <exception cref="InvalidOperationException">The rectangle is empty.</exception>
	public void Offset(double offsetX, double offsetY)
	{
		if (IsEmpty) throw new InvalidOperationException("Cannot modify this property on the Empty Rectangle.");
		X += offsetX;
		Y += offsetY;
	}

	/// <summary>Moves the rectangle by <paramref name="offsetVector"/>.</summary>
	public void Offset(Vector offsetVector) => Offset(offsetVector.X, offsetVector.Y);
}
