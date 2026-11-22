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
public abstract partial class Transform : GeneralTransform
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

