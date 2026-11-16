using System;

namespace System.Windows.Media
{
	/// <summary>
	/// A 3×3 affine matrix for 2D transformations, stored as the six non-trivial elements.
	/// Mirrors <c>System.Windows.Media.Matrix</c>.
	/// Layout (row-major):  | M11  M12  0 |
	///                      | M21  M22  0 |
	///                      | OffsetX  OffsetY  1 |
	/// </summary>
	public struct Matrix : IEquatable<Matrix>
	{
		/// <summary>M11 — X scale component.</summary>
		public double M11 { get; set; }
		/// <summary>M12 — X shear component.</summary>
		public double M12 { get; set; }
		/// <summary>M21 — Y shear component.</summary>
		public double M21 { get; set; }
		/// <summary>M22 — Y scale component.</summary>
		public double M22 { get; set; }
		/// <summary>X translation.</summary>
		public double OffsetX { get; set; }
		/// <summary>Y translation.</summary>
		public double OffsetY { get; set; }

		/// <summary>The identity matrix.</summary>
		public static Matrix Identity => new Matrix(1, 0, 0, 1, 0, 0);

		/// <summary>Creates a matrix from its six elements.</summary>
		public Matrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
		{
			M11 = m11; M12 = m12; M21 = m21; M22 = m22;
			OffsetX = offsetX; OffsetY = offsetY;
		}

		/// <summary>Returns true when this is the identity matrix.</summary>
		public bool IsIdentity => M11 == 1 && M12 == 0 && M21 == 0 && M22 == 1
			&& OffsetX == 0 && OffsetY == 0;

		/// <summary>Multiplies two matrices (this × <paramref name="b"/>).</summary>
		public Matrix Multiply(Matrix b) => new Matrix(
			M11 * b.M11 + M12 * b.M21,
			M11 * b.M12 + M12 * b.M22,
			M21 * b.M11 + M22 * b.M21,
			M21 * b.M12 + M22 * b.M22,
			OffsetX * b.M11 + OffsetY * b.M21 + b.OffsetX,
			OffsetX * b.M12 + OffsetY * b.M22 + b.OffsetY);

		/// <summary>Transforms a <see cref="System.Windows.Point"/>.</summary>
		public System.Windows.Point Transform(System.Windows.Point p) => new System.Windows.Point(
			p.X * M11 + p.Y * M21 + OffsetX,
			p.X * M12 + p.Y * M22 + OffsetY);

		/// <summary>Converts to a <see cref="Cairo.Matrix"/>.</summary>
		internal Cairo.Matrix ToCairo() => new Cairo.Matrix(M11, M12, M21, M22, OffsetX, OffsetY);

		/// <inheritdoc/>
		public bool Equals(Matrix o) =>
			M11 == o.M11 && M12 == o.M12 && M21 == o.M21 && M22 == o.M22
			&& OffsetX == o.OffsetX && OffsetY == o.OffsetY;
		/// <inheritdoc/>
		public override bool Equals(object? obj) => obj is Matrix m && Equals(m);
		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				int h = M11.GetHashCode();
				h = h * 397 ^ M12.GetHashCode();
				h = h * 397 ^ M21.GetHashCode();
				h = h * 397 ^ M22.GetHashCode();
				h = h * 397 ^ OffsetX.GetHashCode();
				return h * 397 ^ OffsetY.GetHashCode();
			}
		}
		/// <inheritdoc/>
		public static bool operator ==(Matrix a, Matrix b) => a.Equals(b);
		/// <inheritdoc/>
		public static bool operator !=(Matrix a, Matrix b) => !a.Equals(b);
	}

	/// <summary>Abstract base for WPF 2D transforms.</summary>
	public abstract class Transform
	{
		/// <summary>Returns the affine matrix represented by this transform.</summary>
		public abstract Matrix Value { get; }

		/// <summary>The identity transform (no-op).</summary>
		public static Transform Identity { get; } = new MatrixTransform(Matrix.Identity);

		/// <summary>Applies this transform to the given Cairo context.</summary>
		internal void ApplyToCairo(Cairo.Context cr) => cr.Transform(Value.ToCairo());
	}

	/// <summary>
	/// A transform that wraps a raw <see cref="Matrix"/>.
	/// Mirrors <c>System.Windows.Media.MatrixTransform</c>.
	/// </summary>
	public class MatrixTransform : Transform
	{
		/// <summary>Creates a matrix transform.</summary>
		public MatrixTransform(Matrix matrix) => Matrix = matrix;

		/// <summary>The underlying matrix.</summary>
		public Matrix Matrix { get; set; }

		/// <inheritdoc/>
		public override Matrix Value => Matrix;
	}

	/// <summary>
	/// Translates (moves) content by <see cref="X"/> and <see cref="Y"/>.
	/// Mirrors <c>System.Windows.Media.TranslateTransform</c>.
	/// </summary>
	public class TranslateTransform : Transform
	{
		/// <summary>Creates a translate transform.</summary>
		public TranslateTransform(double x = 0, double y = 0) { X = x; Y = y; }

		/// <summary>X translation in device-independent units.</summary>
		public double X { get; set; }
		/// <summary>Y translation.</summary>
		public double Y { get; set; }

		/// <inheritdoc/>
		public override Matrix Value => new Matrix(1, 0, 0, 1, X, Y);
	}

	/// <summary>
	/// Scales content by <see cref="ScaleX"/> / <see cref="ScaleY"/> around a centre point.
	/// Mirrors <c>System.Windows.Media.ScaleTransform</c>.
	/// </summary>
	public class ScaleTransform : Transform
	{
		/// <summary>Creates a scale transform.</summary>
		public ScaleTransform(double scaleX = 1, double scaleY = 1,
			double centerX = 0, double centerY = 0)
		{
			ScaleX = scaleX; ScaleY = scaleY;
			CenterX = centerX; CenterY = centerY;
		}

		/// <summary>Horizontal scale factor.</summary>
		public double ScaleX { get; set; } = 1;
		/// <summary>Vertical scale factor.</summary>
		public double ScaleY { get; set; } = 1;
		/// <summary>Centre X of the scale operation.</summary>
		public double CenterX { get; set; }
		/// <summary>Centre Y of the scale operation.</summary>
		public double CenterY { get; set; }

		/// <inheritdoc/>
		public override Matrix Value =>
			new Matrix(ScaleX, 0, 0, ScaleY,
				CenterX * (1 - ScaleX),
				CenterY * (1 - ScaleY));
	}

	/// <summary>
	/// Rotates content by <see cref="Angle"/> degrees around a centre point.
	/// Mirrors <c>System.Windows.Media.RotateTransform</c>.
	/// </summary>
	public class RotateTransform : Transform
	{
		/// <summary>Creates a rotate transform.</summary>
		public RotateTransform(double angle = 0, double centerX = 0, double centerY = 0)
		{
			Angle = angle; CenterX = centerX; CenterY = centerY;
		}

		/// <summary>Rotation angle in degrees (clockwise).</summary>
		public double Angle { get; set; }
		/// <summary>Centre X of rotation.</summary>
		public double CenterX { get; set; }
		/// <summary>Centre Y of rotation.</summary>
		public double CenterY { get; set; }

		/// <inheritdoc/>
		public override Matrix Value
		{
			get
			{
				double rad = Angle * Math.PI / 180.0;
				double cos = Math.Cos(rad), sin = Math.Sin(rad);
				return new Matrix(
					cos, sin, -sin, cos,
					CenterX * (1 - cos) + CenterY * sin,
					CenterY * (1 - cos) - CenterX * sin);
			}
		}
	}
}
