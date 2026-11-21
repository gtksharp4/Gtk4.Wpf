using System;
using System.Globalization;

namespace System.Windows.Media;

/// <summary>Inversion, point transformation, equality and text conversion for <see cref="Matrix"/>.</summary>
public partial struct Matrix
{
	/// <summary>Inverts the matrix. Throws <see cref="InvalidOperationException"/> when it is singular.</summary>
	public void Invert()
	{
		double det = Determinant;
		if (!HasInverse)
			throw new InvalidOperationException("Transform is not invertible.");
		double m11 = M11, m12 = M12, m21 = M21, m22 = M22, ox = OffsetX, oy = OffsetY;
		this = new Matrix(m22 / det, -m12 / det, -m21 / det, m11 / det,
			(m21 * oy - m22 * ox) / det, (m12 * ox - m11 * oy) / det);
	}

	/// <summary>Transforms a point by this matrix.</summary>
	public Point Transform(Point point) =>
		new Point(point.X * M11 + point.Y * M21 + OffsetX, point.X * M12 + point.Y * M22 + OffsetY);

	/// <summary>Transforms a vector by this matrix; the translation is not applied.</summary>
	public Vector Transform(Vector vector) =>
		new Vector(vector.X * M11 + vector.Y * M21, vector.X * M12 + vector.Y * M22);

	/// <summary>Transforms an array of points in place.</summary>
	public void Transform(Point[] points)
	{
		if (points == null) return;
		for (int i = 0; i < points.Length; i++)
			points[i] = Transform(points[i]);
	}

	/// <summary>Transforms an array of vectors in place.</summary>
	public void Transform(Vector[] vectors)
	{
		if (vectors == null) return;
		for (int i = 0; i < vectors.Length; i++)
			vectors[i] = Transform(vectors[i]);
	}

	/// <summary>Compares two matrices for exact equality.</summary>
	public static bool operator ==(Matrix matrix1, Matrix matrix2) =>
		matrix1.M11 == matrix2.M11 && matrix1.M12 == matrix2.M12 && matrix1.M21 == matrix2.M21
		&& matrix1.M22 == matrix2.M22 && matrix1.OffsetX == matrix2.OffsetX && matrix1.OffsetY == matrix2.OffsetY;

	/// <summary>Compares two matrices for inequality.</summary>
	public static bool operator !=(Matrix matrix1, Matrix matrix2) => !(matrix1 == matrix2);

	/// <summary>Compares two matrices for equality (NaN equals NaN).</summary>
	public static bool Equals(Matrix matrix1, Matrix matrix2) =>
		matrix1.M11.Equals(matrix2.M11) && matrix1.M12.Equals(matrix2.M12) && matrix1.M21.Equals(matrix2.M21)
		&& matrix1.M22.Equals(matrix2.M22) && matrix1.OffsetX.Equals(matrix2.OffsetX)
		&& matrix1.OffsetY.Equals(matrix2.OffsetY);

	/// <inheritdoc/>
	public bool Equals(Matrix value) => Equals(this, value);

	/// <inheritdoc/>
	public override bool Equals(object? o) => o is Matrix m && Equals(this, m);

	/// <inheritdoc/>
	public override int GetHashCode() =>
		IsIdentity ? 0 : M11.GetHashCode() ^ M12.GetHashCode() ^ M21.GetHashCode() ^ M22.GetHashCode()
			^ OffsetX.GetHashCode() ^ OffsetY.GetHashCode();

	/// <summary>Converts "Identity" or six comma/whitespace separated numbers to a matrix.</summary>
	public static Matrix Parse(string source)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (source.Trim() == "Identity")
			return Identity;
		var reader = new TokenizerHelper.Reader(source);
		var value = new Matrix(reader.NextDouble(), reader.NextDouble(), reader.NextDouble(),
			reader.NextDouble(), reader.NextDouble(), reader.NextDouble());
		reader.LastRequired();
		return value;
	}

	/// <inheritdoc/>
	public override string ToString() => ToString(null, null);

	/// <summary>Formats the matrix with the given provider's list separator.</summary>
	public string ToString(IFormatProvider? provider) => ToString(null, provider);

	/// <inheritdoc/>
	public string ToString(string? format, IFormatProvider? provider)
	{
		if (IsIdentity) return "Identity";
		char s = TokenizerHelper.GetNumericListSeparator(provider);
		return M11.ToString(format, provider) + s + M12.ToString(format, provider) + s
			+ M21.ToString(format, provider) + s + M22.ToString(format, provider) + s
			+ OffsetX.ToString(format, provider) + s + OffsetY.ToString(format, provider);
	}
}
