using System;
using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(System.Windows.Media.Matrix))]

namespace System.Windows.Media;

static class MatrixCairo
{
	/// <summary>Converts to a <see cref="Cairo.Matrix"/>.</summary>
	internal static Cairo.Matrix ToCairo(this Matrix m) =>
		new Cairo.Matrix(m.M11, m.M12, m.M21, m.M22, m.OffsetX, m.OffsetY);
}

/// <summary>Abstract base for WPF 2D transforms.</summary>
public abstract class Transform : System.Windows.Media.Animation.Animatable
{
	/// <summary>Returns the affine matrix represented by this transform.</summary>
	public abstract Matrix Value { get; }

	/// <summary>The identity transform (no-op).</summary>
	public static Transform Identity { get; } = MakeIdentity();

	static Transform MakeIdentity()
	{
		var t = new MatrixTransform(Matrix.Identity);
		t.Freeze();
		return t;
	}

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
	public Matrix Matrix
	{
		get => _matrix;
		set
		{
			ThrowIfFrozen();
			_matrix = value;
			OnChanged();
		}
	}

	Matrix _matrix;

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new MatrixTransform(Matrix.Identity);

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly) =>
		((MatrixTransform)destination)._matrix = _matrix;

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
	public TranslateTransform(double x = 0, double y = 0)
	{
		if (x != 0) X = x;
		if (y != 0) Y = y;
	}

	/// <summary>X translation in device-independent units.</summary>
	public double X
	{
		get => (double)GetValue(XProperty)!;
		set => SetValue(XProperty, value);
	}

	/// <summary>Y translation.</summary>
	public double Y
	{
		get => (double)GetValue(YProperty)!;
		set => SetValue(YProperty, value);
	}

	/// <summary>Identifies <see cref="X"/>.</summary>
	public static readonly DependencyProperty XProperty = DependencyProperty.Register(
		nameof(X), typeof(double), typeof(TranslateTransform), new PropertyMetadata(0.0));

	/// <summary>Identifies <see cref="Y"/>.</summary>
	public static readonly DependencyProperty YProperty = DependencyProperty.Register(
		nameof(Y), typeof(double), typeof(TranslateTransform), new PropertyMetadata(0.0));

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new TranslateTransform();

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
	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new ScaleTransform();

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		var t = (ScaleTransform)destination;
		t.ScaleX = ScaleX; t.ScaleY = ScaleY; t.CenterX = CenterX; t.CenterY = CenterY;
	}

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
	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new RotateTransform();

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		var t = (RotateTransform)destination;
		t.Angle = Angle; t.CenterX = CenterX; t.CenterY = CenterY;
	}

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