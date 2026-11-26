using System;

namespace System.Windows.Media;

/// <summary>
/// A 3x3 affine matrix for 2-D transformations, stored as its six non-trivial elements.
/// Mirrors <c>System.Windows.Media.Matrix</c>; <c>default(Matrix)</c> is the identity, as in WPF.
/// </summary>
public partial struct Matrix : IFormattable, IEquatable<Matrix>
{
	// Zero fields plus _init == false means identity, so default(Matrix) is the identity.
	double _m11, _m12, _m21, _m22, _offsetX, _offsetY;
	bool _init;

	/// <summary>Creates a matrix from its six elements.</summary>
	public Matrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
	{
		_m11 = m11; _m12 = m12; _m21 = m21; _m22 = m22;
		_offsetX = offsetX; _offsetY = offsetY;
		_init = true;
	}

	void EnsureInit()
	{
		if (_init) return;
		_m11 = 1;
		_m22 = 1;
		_init = true;
	}

	/// <summary>The identity matrix.</summary>
	public static Matrix Identity => new Matrix(1, 0, 0, 1, 0, 0);

	/// <summary>Resets this matrix to the identity.</summary>
	public void SetIdentity() => this = Identity;

	/// <summary>Whether this is the identity matrix.</summary>
	public bool IsIdentity => M11 == 1 && M12 == 0 && M21 == 0 && M22 == 1 && OffsetX == 0 && OffsetY == 0;

	/// <summary>X scale component.</summary>
	public double M11 { get => _init ? _m11 : 1; set { EnsureInit(); _m11 = value; } }

	/// <summary>Y shear component.</summary>
	public double M12 { get => _m12; set { EnsureInit(); _m12 = value; } }

	/// <summary>X shear component.</summary>
	public double M21 { get => _m21; set { EnsureInit(); _m21 = value; } }

	/// <summary>Y scale component.</summary>
	public double M22 { get => _init ? _m22 : 1; set { EnsureInit(); _m22 = value; } }

	/// <summary>X translation.</summary>
	public double OffsetX { get => _offsetX; set { EnsureInit(); _offsetX = value; } }

	/// <summary>Y translation.</summary>
	public double OffsetY { get => _offsetY; set { EnsureInit(); _offsetY = value; } }

	/// <summary>The determinant of the matrix.</summary>
	public double Determinant => M11 * M22 - M12 * M21;

	/// <summary>Whether the matrix can be inverted.</summary>
	public bool HasInverse => Math.Abs(Determinant) >= 10.0 * 2.2204460492503131E-16;

	/// <summary>Multiplies two matrices (<paramref name="matrix1"/> x <paramref name="matrix2"/>).</summary>
	public static Matrix Multiply(Matrix matrix1, Matrix matrix2) => new Matrix(
		matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21,
		matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22,
		matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21,
		matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22,
		matrix1.OffsetX * matrix2.M11 + matrix1.OffsetY * matrix2.M21 + matrix2.OffsetX,
		matrix1.OffsetX * matrix2.M12 + matrix1.OffsetY * matrix2.M22 + matrix2.OffsetY);

	/// <summary>Multiplies this matrix by <paramref name="b"/> (this x b).</summary>
	public Matrix Multiply(Matrix b) => Multiply(this, b);

	/// <summary>Multiplies two matrices.</summary>
	public static Matrix operator *(Matrix trans1, Matrix trans2) => Multiply(trans1, trans2);

	/// <summary>Appends a matrix: this = this x matrix.</summary>
	public void Append(Matrix matrix) => this = Multiply(this, matrix);

	/// <summary>Prepends a matrix: this = matrix x this.</summary>
	public void Prepend(Matrix matrix) => this = Multiply(matrix, this);

	/// <summary>Appends a rotation about the origin, in degrees.</summary>
	public void Rotate(double angle) => Append(CreateRotation(angle, 0, 0));

	/// <summary>Prepends a rotation about the origin, in degrees.</summary>
	public void RotatePrepend(double angle) => Prepend(CreateRotation(angle, 0, 0));

	/// <summary>Appends a rotation about a point, in degrees.</summary>
	public void RotateAt(double angle, double centerX, double centerY) =>
		Append(CreateRotation(angle, centerX, centerY));

	/// <summary>Prepends a rotation about a point, in degrees.</summary>
	public void RotateAtPrepend(double angle, double centerX, double centerY) =>
		Prepend(CreateRotation(angle, centerX, centerY));

	/// <summary>Appends a scale about the origin.</summary>
	public void Scale(double scaleX, double scaleY) => Append(new Matrix(scaleX, 0, 0, scaleY, 0, 0));

	/// <summary>Prepends a scale about the origin.</summary>
	public void ScalePrepend(double scaleX, double scaleY) => Prepend(new Matrix(scaleX, 0, 0, scaleY, 0, 0));

	/// <summary>Appends a scale about a point.</summary>
	public void ScaleAt(double scaleX, double scaleY, double centerX, double centerY) =>
		Append(CreateScaleAt(scaleX, scaleY, centerX, centerY));

	/// <summary>Prepends a scale about a point.</summary>
	public void ScaleAtPrepend(double scaleX, double scaleY, double centerX, double centerY) =>
		Prepend(CreateScaleAt(scaleX, scaleY, centerX, centerY));

	/// <summary>Appends a skew, in degrees.</summary>
	public void Skew(double skewX, double skewY) => Append(CreateSkew(skewX, skewY));

	/// <summary>Prepends a skew, in degrees.</summary>
	public void SkewPrepend(double skewX, double skewY) => Prepend(CreateSkew(skewX, skewY));

	/// <summary>Appends a translation.</summary>
	public void Translate(double offsetX, double offsetY) => Append(new Matrix(1, 0, 0, 1, offsetX, offsetY));

	/// <summary>Prepends a translation.</summary>
	public void TranslatePrepend(double offsetX, double offsetY) =>
		Prepend(new Matrix(1, 0, 0, 1, offsetX, offsetY));

	static Matrix CreateRotation(double angle, double centerX, double centerY)
	{
		angle %= 360.0;
		double radians = angle * (Math.PI / 180.0);
		double sin = Math.Sin(radians);
		double cos = Math.Cos(radians);
		return new Matrix(cos, sin, -sin, cos,
			centerX * (1 - cos) + centerY * sin,
			centerY * (1 - cos) - centerX * sin);
	}

	static Matrix CreateScaleAt(double scaleX, double scaleY, double centerX, double centerY) =>
		new Matrix(scaleX, 0, 0, scaleY, centerX - scaleX * centerX, centerY - scaleY * centerY);

	static Matrix CreateSkew(double skewX, double skewY)
	{
		double x = (skewX % 360) * (Math.PI / 180.0);
		double y = (skewY % 360) * (Math.PI / 180.0);
		return new Matrix(1, Math.Tan(y), Math.Tan(x), 1, 0, 0);
	}
}
