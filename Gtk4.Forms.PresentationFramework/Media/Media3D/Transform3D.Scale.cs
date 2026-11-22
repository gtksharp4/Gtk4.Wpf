namespace System.Windows.Media.Media3D;

/// <summary>Scales 3D content about a centre.</summary>
public sealed class ScaleTransform3D : Transform3D
{
	/// <summary>Identifies <see cref="ScaleX"/>.</summary>
	public static readonly DependencyProperty ScaleXProperty =
		Media3DProps.Register(nameof(ScaleX), typeof(double), typeof(ScaleTransform3D), 1.0);

	/// <summary>Identifies <see cref="ScaleY"/>.</summary>
	public static readonly DependencyProperty ScaleYProperty =
		Media3DProps.Register(nameof(ScaleY), typeof(double), typeof(ScaleTransform3D), 1.0);

	/// <summary>Identifies <see cref="ScaleZ"/>.</summary>
	public static readonly DependencyProperty ScaleZProperty =
		Media3DProps.Register(nameof(ScaleZ), typeof(double), typeof(ScaleTransform3D), 1.0);

	/// <summary>Identifies <see cref="CenterX"/>.</summary>
	public static readonly DependencyProperty CenterXProperty =
		Media3DProps.Register(nameof(CenterX), typeof(double), typeof(ScaleTransform3D), 0.0);

	/// <summary>Identifies <see cref="CenterY"/>.</summary>
	public static readonly DependencyProperty CenterYProperty =
		Media3DProps.Register(nameof(CenterY), typeof(double), typeof(ScaleTransform3D), 0.0);

	/// <summary>Identifies <see cref="CenterZ"/>.</summary>
	public static readonly DependencyProperty CenterZProperty =
		Media3DProps.Register(nameof(CenterZ), typeof(double), typeof(ScaleTransform3D), 0.0);

	/// <summary>Creates an identity scale.</summary>
	public ScaleTransform3D() { }

	/// <summary>Creates a scale about the origin.</summary>
	public ScaleTransform3D(Vector3D scale) : this(scale.X, scale.Y, scale.Z) { }

	/// <summary>Creates a scale about <paramref name="scaleCenter"/>.</summary>
	public ScaleTransform3D(Vector3D scale, Point3D scaleCenter)
		: this(scale.X, scale.Y, scale.Z, scaleCenter.X, scaleCenter.Y, scaleCenter.Z) { }

	/// <summary>Creates a scale about the origin.</summary>
	public ScaleTransform3D(double scaleX, double scaleY, double scaleZ)
	{
		ScaleX = scaleX;
		ScaleY = scaleY;
		ScaleZ = scaleZ;
	}

	/// <summary>Creates a scale about a centre.</summary>
	public ScaleTransform3D(
		double scaleX, double scaleY, double scaleZ, double centerX, double centerY, double centerZ)
		: this(scaleX, scaleY, scaleZ)
	{
		CenterX = centerX;
		CenterY = centerY;
		CenterZ = centerZ;
	}

	/// <summary>The X scale.</summary>
	public double ScaleX
	{
		get => (double)GetValue(ScaleXProperty)!;
		set => SetValue(ScaleXProperty, value);
	}

	/// <summary>The Y scale.</summary>
	public double ScaleY
	{
		get => (double)GetValue(ScaleYProperty)!;
		set => SetValue(ScaleYProperty, value);
	}

	/// <summary>The Z scale.</summary>
	public double ScaleZ
	{
		get => (double)GetValue(ScaleZProperty)!;
		set => SetValue(ScaleZProperty, value);
	}

	/// <summary>The X of the centre.</summary>
	public double CenterX
	{
		get => (double)GetValue(CenterXProperty)!;
		set => SetValue(CenterXProperty, value);
	}

	/// <summary>The Y of the centre.</summary>
	public double CenterY
	{
		get => (double)GetValue(CenterYProperty)!;
		set => SetValue(CenterYProperty, value);
	}

	/// <summary>The Z of the centre.</summary>
	public double CenterZ
	{
		get => (double)GetValue(CenterZProperty)!;
		set => SetValue(CenterZProperty, value);
	}

	/// <inheritdoc/>
	public override Matrix3D Value
	{
		get
		{
			var m = Matrix3D.Identity;
			m.ScaleAt(new Vector3D(ScaleX, ScaleY, ScaleZ), new Point3D(CenterX, CenterY, CenterZ));
			return m;
		}
	}

	/// <inheritdoc/>
	public override bool IsAffine => true;

	/// <summary>Creates a modifiable clone.</summary>
	public new ScaleTransform3D Clone() => (ScaleTransform3D)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new ScaleTransform3D();
}

/// <summary>Applies a raw <see cref="Matrix3D"/>.</summary>
public sealed class MatrixTransform3D : Transform3D
{
	/// <summary>Identifies <see cref="Matrix"/>.</summary>
	public static readonly DependencyProperty MatrixProperty =
		Media3DProps.Register(nameof(Matrix), typeof(Matrix3D), typeof(MatrixTransform3D), Matrix3D.Identity);

	/// <summary>Creates an identity transform.</summary>
	public MatrixTransform3D() { }

	/// <summary>Creates a transform from a matrix.</summary>
	public MatrixTransform3D(Matrix3D matrix) => Matrix = matrix;

	/// <summary>The matrix.</summary>
	public Matrix3D Matrix
	{
		get => (Matrix3D)GetValue(MatrixProperty)!;
		set => SetValue(MatrixProperty, value);
	}

	/// <inheritdoc/>
	public override Matrix3D Value => Matrix;

	/// <inheritdoc/>
	public override bool IsAffine => Matrix.IsAffine;

	/// <summary>Creates a modifiable clone.</summary>
	public new MatrixTransform3D Clone() => (MatrixTransform3D)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new MatrixTransform3D();
}
