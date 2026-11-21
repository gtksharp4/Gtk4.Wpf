using System;
using System.Windows.Media;

namespace System.Windows;

/// <summary>Represents a displacement in 2-D space. Mirrors <c>System.Windows.Vector</c>.</summary>
public struct Vector : IFormattable, IEquatable<Vector>
{
	/// <summary>Creates a vector.</summary>
	public Vector(double x, double y)
	{
		X = x;
		Y = y;
	}

	/// <summary>The X component.</summary>
	public double X { get; set; }

	/// <summary>The Y component.</summary>
	public double Y { get; set; }

	/// <summary>The length of the vector.</summary>
	public double Length => Math.Sqrt(X * X + Y * Y);

	/// <summary>The squared length of the vector.</summary>
	public double LengthSquared => X * X + Y * Y;

	/// <summary>Scales the vector to unit length. A zero vector becomes NaN components, as in WPF.</summary>
	public void Normalize()
	{
		// Pre-scale by the largest component so squaring cannot overflow.
		this /= Math.Max(Math.Abs(X), Math.Abs(Y));
		this /= Length;
	}

	/// <summary>Reverses the vector's direction.</summary>
	public void Negate()
	{
		X = -X;
		Y = -Y;
	}

	/// <summary>The cross product (determinant) of two vectors.</summary>
	public static double CrossProduct(Vector vector1, Vector vector2) => vector1.X * vector2.Y - vector1.Y * vector2.X;

	/// <summary>The signed angle in degrees from <paramref name="vector1"/> to <paramref name="vector2"/>.</summary>
	public static double AngleBetween(Vector vector1, Vector vector2)
	{
		double sin = vector1.X * vector2.Y - vector2.X * vector1.Y;
		double cos = vector1.X * vector2.X + vector1.Y * vector2.Y;
		return Math.Atan2(sin, cos) * (180 / Math.PI);
	}

	/// <summary>The determinant of the matrix whose rows are the two vectors.</summary>
	public static double Determinant(Vector vector1, Vector vector2) => CrossProduct(vector1, vector2);

	/// <summary>Adds two vectors.</summary>
	public static Vector Add(Vector vector1, Vector vector2)
		=> new Vector(vector1.X + vector2.X, vector1.Y + vector2.Y);

	/// <summary>Translates a point by a vector.</summary>
	public static Point Add(Vector vector, Point point) => new Point(point.X + vector.X, point.Y + vector.Y);

	/// <summary>Subtracts one vector from another.</summary>
	public static Vector Subtract(Vector vector1, Vector vector2) =>
		new Vector(vector1.X - vector2.X, vector1.Y - vector2.Y);

	/// <summary>Multiplies a vector by a scalar.</summary>
	public static Vector Multiply(Vector vector, double scalar) => new Vector(vector.X * scalar, vector.Y * scalar);

	/// <summary>Multiplies a scalar by a vector.</summary>
	public static Vector Multiply(double scalar, Vector vector) => new Vector(vector.X * scalar, vector.Y * scalar);

	/// <summary>Divides a vector by a scalar.</summary>
	public static Vector Divide(Vector vector, double scalar) => Multiply(vector, 1.0 / scalar);

	/// <summary>The dot product of two vectors.</summary>
	public static double Multiply(Vector vector1, Vector vector2) => vector1.X * vector2.X + vector1.Y * vector2.Y;

	/// <summary>Transforms a vector by a matrix (the matrix translation is ignored).</summary>
	public static Vector Multiply(Vector vector, Matrix matrix) => matrix.Transform(vector);

	/// <summary>Negates a vector.</summary>
	public static Vector operator -(Vector vector) => new Vector(-vector.X, -vector.Y);

	/// <summary>Adds two vectors.</summary>
	public static Vector operator +(Vector vector1, Vector vector2) => Add(vector1, vector2);

	/// <summary>Subtracts one vector from another.</summary>
	public static Vector operator -(Vector vector1, Vector vector2) => Subtract(vector1, vector2);

	/// <summary>Translates a point by a vector.</summary>
	public static Point operator +(Vector vector, Point point) => Add(vector, point);

	/// <summary>Multiplies a vector by a scalar.</summary>
	public static Vector operator *(Vector vector, double scalar) => Multiply(vector, scalar);

	/// <summary>Multiplies a scalar by a vector.</summary>
	public static Vector operator *(double scalar, Vector vector) => Multiply(scalar, vector);

	/// <summary>Divides a vector by a scalar.</summary>
	public static Vector operator /(Vector vector, double scalar) => Divide(vector, scalar);

	/// <summary>The dot product of two vectors.</summary>
	public static double operator *(Vector vector1, Vector vector2) => Multiply(vector1, vector2);

	/// <summary>Transforms a vector by a matrix.</summary>
	public static Vector operator *(Vector vector, Matrix matrix) => matrix.Transform(vector);

	/// <summary>Converts the vector to a size using absolute values.</summary>
	public static explicit operator Size(Vector vector) => new Size(Math.Abs(vector.X), Math.Abs(vector.Y));

	/// <summary>Converts the vector to a point.</summary>
	public static explicit operator Point(Vector vector) => new Point(vector.X, vector.Y);

	/// <summary>Compares two vectors for exact equality.</summary>
	public static bool operator ==(Vector vector1, Vector vector2) =>
		vector1.X == vector2.X && vector1.Y == vector2.Y;

	/// <summary>Compares two vectors for inequality.</summary>
	public static bool operator !=(Vector vector1, Vector vector2) => !(vector1 == vector2);

	/// <summary>Compares two vectors for equality (NaN equals NaN).</summary>
	public static bool Equals(Vector vector1, Vector vector2) =>
		vector1.X.Equals(vector2.X) && vector1.Y.Equals(vector2.Y);

	/// <inheritdoc/>
	public bool Equals(Vector value) => Equals(this, value);

	/// <inheritdoc/>
	public override bool Equals(object? o) => o is Vector v && Equals(this, v);

	/// <inheritdoc/>
	public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

	/// <summary>Converts a string of two numbers separated by a comma or whitespace to a vector.</summary>
	public static Vector Parse(string source)
	{
		var reader = new TokenizerHelper.Reader(source);
		var value = new Vector(reader.NextDouble(), reader.NextDouble());
		reader.LastRequired();
		return value;
	}

	/// <inheritdoc/>
	public override string ToString() => ToString(null, null);

	/// <summary>Formats the vector with the given provider's list separator.</summary>
	public string ToString(IFormatProvider? provider) => ToString(null, provider);

	/// <inheritdoc/>
	public string ToString(string? format, IFormatProvider? provider)
	{
		char sep = TokenizerHelper.GetNumericListSeparator(provider);
		return X.ToString(format, provider) + sep + Y.ToString(format, provider);
	}
}
