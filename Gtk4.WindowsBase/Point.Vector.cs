using System;
using System.Globalization;
using System.Windows.Media;

namespace System.Windows;

/// <summary>Vector, matrix and text-conversion members of <see cref="Point"/>.</summary>
public partial struct Point : IFormattable
{
	/// <summary>Moves the point by the given offsets.</summary>
	public void Offset(double offsetX, double offsetY)
	{
		X += offsetX;
		Y += offsetY;
	}

	/// <summary>Adds a vector to a point.</summary>
	public static Point Add(Point point, Vector vector) => new Point(point.X + vector.X, point.Y + vector.Y);

	/// <summary>Subtracts a vector from a point.</summary>
	public static Point Subtract(Point point, Vector vector) => new Point(point.X - vector.X, point.Y - vector.Y);

	/// <summary>Returns the vector from <paramref name="point2"/> to <paramref name="point1"/>.</summary>
	public static Vector Subtract(Point point1, Point point2) => new Vector(point1.X - point2.X, point1.Y - point2.Y);

	/// <summary>Transforms a point by a matrix.</summary>
	public static Point Multiply(Point point, Matrix matrix) => matrix.Transform(point);

	/// <summary>Converts a string of two numbers separated by a comma or whitespace to a point.</summary>
	public static Point Parse(string source)
	{
		var reader = new TokenizerHelper.Reader(source);
		var value = new Point(reader.NextDouble(), reader.NextDouble());
		reader.LastRequired();
		return value;
	}

	/// <summary>Adds a vector to a point.</summary>
	public static Point operator +(Point point, Vector vector) => Add(point, vector);

	/// <summary>Subtracts a vector from a point.</summary>
	public static Point operator -(Point point, Vector vector) => Subtract(point, vector);

	/// <summary>Returns the vector between two points.</summary>
	public static Vector operator -(Point point1, Point point2) => Subtract(point1, point2);

	/// <summary>Transforms a point by a matrix.</summary>
	public static Point operator *(Point point, Matrix matrix) => matrix.Transform(point);

	/// <summary>Converts the point to a size using absolute values.</summary>
	public static explicit operator Size(Point point) => new Size(Math.Abs(point.X), Math.Abs(point.Y));

	/// <summary>Converts the point to a vector.</summary>
	public static explicit operator Vector(Point point) => new Vector(point.X, point.Y);

	/// <summary>Formats the point with the given provider's list separator.</summary>
	public string ToString(IFormatProvider? provider) => ToString(null, provider);

	/// <inheritdoc/>
	public string ToString(string? format, IFormatProvider? provider)
	{
		char sep = TokenizerHelper.GetNumericListSeparator(provider);
		return X.ToString(format, provider) + sep + Y.ToString(format, provider);
	}
}
