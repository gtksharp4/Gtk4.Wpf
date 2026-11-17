using System;

namespace System.Windows;

/// <summary>Represents the thickness of a frame around a rectangle.</summary>
public struct Thickness : IEquatable<Thickness>
{
	/// <summary>Width of the left side.</summary>
	public double Left { get; set; }
	/// <summary>Width of the top side.</summary>
	public double Top { get; set; }
	/// <summary>Width of the right side.</summary>
	public double Right { get; set; }
	/// <summary>Width of the bottom side.</summary>
	public double Bottom { get; set; }

	/// <summary>Creates a uniform thickness.</summary>
	public Thickness(double uniformLength)
		: this(uniformLength, uniformLength, uniformLength, uniformLength) { }

	/// <summary>Creates a thickness with specific values for each side.</summary>
	public Thickness(double left, double top, double right, double bottom)
	{
		Left = left; Top = top; Right = right; Bottom = bottom;
	}

	/// <inheritdoc/>
	public bool Equals(Thickness other) =>
		Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;
	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is Thickness t && Equals(t);
	/// <inheritdoc/>
	public override int GetHashCode() => PrimitivesHashHelper.CombineHash(Left.GetHashCode(), Top.GetHashCode(), Right.GetHashCode(), Bottom.GetHashCode());
	/// <inheritdoc/>
	public static bool operator ==(Thickness a, Thickness b) => a.Equals(b);
	/// <inheritdoc/>
	public static bool operator !=(Thickness a, Thickness b) => !a.Equals(b);
	/// <inheritdoc/>
	public override string ToString() => $"{Left},{Top},{Right},{Bottom}";
}

/// <summary>Represents the size of an element.</summary>
public struct Size : IEquatable<Size>
{
	double _width, _height;

	/// <summary>A <see cref="Size"/> with both dimensions positive infinity — the WPF "empty" sentinel.</summary>
	public static readonly Size Empty = new Size(double.PositiveInfinity, double.PositiveInfinity);

	/// <summary>Width of the size.</summary>
	public double Width { get => _width; set => _width = value; }

	/// <summary>Height of the size.</summary>
	public double Height { get => _height; set => _height = value; }

	/// <summary>Returns <c>true</c> when this instance equals <see cref="Empty"/>.</summary>
	public bool IsEmpty => double.IsPositiveInfinity(_width) && double.IsPositiveInfinity(_height);

	/// <summary>Creates a size.</summary>
	public Size(double width, double height) { _width = width; _height = height; }

	/// <inheritdoc/>
	public bool Equals(Size other) => _width == other._width && _height == other._height;
	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is Size s && Equals(s);
	/// <inheritdoc/>
	public override int GetHashCode() => PrimitivesHashHelper.CombineHash(_width.GetHashCode(), _height.GetHashCode());
	/// <inheritdoc/>
	public static bool operator ==(Size a, Size b) => a.Equals(b);
	/// <inheritdoc/>
	public static bool operator !=(Size a, Size b) => !a.Equals(b);
	/// <inheritdoc/>
	public override string ToString() => $"{_width},{_height}";
}

/// <summary>Represents a rectangle defined by a position and a size.</summary>
public struct Rect : IEquatable<Rect>
{
	/// <summary>X coordinate of the top-left corner.</summary>
	public double X { get; set; }
	/// <summary>Y coordinate of the top-left corner.</summary>
	public double Y { get; set; }
	/// <summary>Width of the rectangle.</summary>
	public double Width { get; set; }
	/// <summary>Height of the rectangle.</summary>
	public double Height { get; set; }

	/// <summary>Creates a <see cref="Rect"/> from position and size.</summary>
	public Rect(double x, double y, double width, double height)
	{
		X = x; Y = y; Width = width; Height = height;
	}

	/// <summary>Creates a <see cref="Rect"/> from a <see cref="Point"/> and a <see cref="Size"/>.</summary>
	public Rect(Point location, Size size)
		: this(location.X, location.Y, size.Width, size.Height) { }

	/// <inheritdoc/>
	public bool Equals(Rect other) =>
		X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is Rect r && Equals(r);
	/// <inheritdoc/>
	public override int GetHashCode() => PrimitivesHashHelper.CombineHash(X.GetHashCode(), Y.GetHashCode(), Width.GetHashCode(), Height.GetHashCode());
	/// <inheritdoc/>
	public static bool operator ==(Rect a, Rect b) => a.Equals(b);
	/// <inheritdoc/>
	public static bool operator !=(Rect a, Rect b) => !a.Equals(b);
	/// <inheritdoc/>
	public override string ToString() => $"{X},{Y},{Width},{Height}";
}

/// <summary>Represents an X,Y coordinate pair in 2D space.</summary>
public struct Point : IEquatable<Point>
{
	/// <summary>X coordinate.</summary>
	public double X { get; set; }
	/// <summary>Y coordinate.</summary>
	public double Y { get; set; }

	/// <summary>Creates a point.</summary>
	public Point(double x, double y) { X = x; Y = y; }

	/// <inheritdoc/>
	public bool Equals(Point other) => X == other.X && Y == other.Y;
	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is Point p && Equals(p);
	/// <inheritdoc/>
	public override int GetHashCode() => PrimitivesHashHelper.CombineHash(X.GetHashCode(), Y.GetHashCode());
	/// <inheritdoc/>
	public static bool operator ==(Point a, Point b) => a.Equals(b);
	/// <inheritdoc/>
	public static bool operator !=(Point a, Point b) => !a.Equals(b);
	/// <inheritdoc/>
	public override string ToString() => $"{X},{Y}";
}

// ── Shared helper (netstandard2.0 lacks System.HashCode) ─────────────────────────
static class PrimitivesHashHelper
{
	internal static int CombineHash(int h1, int h2)
	{
		unchecked { return ((h1 << 5) + h1) ^ h2; }
	}
	internal static int CombineHash(int h1, int h2, int h3, int h4)
		=> CombineHash(CombineHash(h1, h2), CombineHash(h3, h4));
}