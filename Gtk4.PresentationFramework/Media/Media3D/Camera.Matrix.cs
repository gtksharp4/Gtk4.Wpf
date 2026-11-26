namespace System.Windows.Media.Media3D;

/// <summary>A camera given by explicit view and projection matrices.</summary>
public sealed class MatrixCamera : Camera
{
	/// <summary>Identifies <see cref="ViewMatrix"/>.</summary>
	public static readonly DependencyProperty ViewMatrixProperty = Media3DProps.Register(
		nameof(ViewMatrix), typeof(Matrix3D), typeof(MatrixCamera), Matrix3D.Identity);

	/// <summary>Identifies <see cref="ProjectionMatrix"/>.</summary>
	public static readonly DependencyProperty ProjectionMatrixProperty = Media3DProps.Register(
		nameof(ProjectionMatrix), typeof(Matrix3D), typeof(MatrixCamera), Matrix3D.Identity);

	/// <summary>Creates an identity camera.</summary>
	public MatrixCamera() { }

	/// <summary>Creates a camera.</summary>
	public MatrixCamera(Matrix3D viewMatrix, Matrix3D projectionMatrix)
	{
		ViewMatrix = viewMatrix;
		ProjectionMatrix = projectionMatrix;
	}

	/// <summary>The view matrix.</summary>
	public Matrix3D ViewMatrix
	{
		get => (Matrix3D)GetValue(ViewMatrixProperty)!;
		set => SetValue(ViewMatrixProperty, value);
	}

	/// <summary>The projection matrix.</summary>
	public Matrix3D ProjectionMatrix
	{
		get => (Matrix3D)GetValue(ProjectionMatrixProperty)!;
		set => SetValue(ProjectionMatrixProperty, value);
	}

	internal override Matrix3D ViewCore() => ViewMatrix;

	internal override Matrix3D GetProjectionMatrix(double aspectRatio) => ProjectionMatrix;

	/// <summary>Creates a modifiable clone.</summary>
	public new MatrixCamera Clone() => (MatrixCamera)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new MatrixCamera();
}
