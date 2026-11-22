namespace System.Windows.Media.Media3D;

/// <summary>A camera that looks from a position along a direction.</summary>
public abstract class ProjectionCamera : Camera
{
	/// <summary>Identifies <see cref="Position"/>.</summary>
	public static readonly DependencyProperty PositionProperty = Media3DProps.Register(
		nameof(Position), typeof(Point3D), typeof(ProjectionCamera), new Point3D(0, 0, 0));

	/// <summary>Identifies <see cref="LookDirection"/>.</summary>
	public static readonly DependencyProperty LookDirectionProperty = Media3DProps.Register(
		nameof(LookDirection), typeof(Vector3D), typeof(ProjectionCamera), new Vector3D(0, 0, -1));

	/// <summary>Identifies <see cref="UpDirection"/>.</summary>
	public static readonly DependencyProperty UpDirectionProperty = Media3DProps.Register(
		nameof(UpDirection), typeof(Vector3D), typeof(ProjectionCamera), new Vector3D(0, 1, 0));

	/// <summary>Identifies <see cref="NearPlaneDistance"/>.</summary>
	public static readonly DependencyProperty NearPlaneDistanceProperty = Media3DProps.Register(
		nameof(NearPlaneDistance), typeof(double), typeof(ProjectionCamera), 0.125);

	/// <summary>Identifies <see cref="FarPlaneDistance"/>.</summary>
	public static readonly DependencyProperty FarPlaneDistanceProperty = Media3DProps.Register(
		nameof(FarPlaneDistance), typeof(double), typeof(ProjectionCamera), double.PositiveInfinity);

	internal ProjectionCamera() { }

	/// <summary>The camera position.</summary>
	public Point3D Position
	{
		get => (Point3D)GetValue(PositionProperty)!;
		set => SetValue(PositionProperty, value);
	}

	/// <summary>The direction the camera looks in.</summary>
	public Vector3D LookDirection
	{
		get => (Vector3D)GetValue(LookDirectionProperty)!;
		set => SetValue(LookDirectionProperty, value);
	}

	/// <summary>The up direction.</summary>
	public Vector3D UpDirection
	{
		get => (Vector3D)GetValue(UpDirectionProperty)!;
		set => SetValue(UpDirectionProperty, value);
	}

	/// <summary>The distance to the near clipping plane.</summary>
	public double NearPlaneDistance
	{
		get => (double)GetValue(NearPlaneDistanceProperty)!;
		set => SetValue(NearPlaneDistanceProperty, value);
	}

	/// <summary>The distance to the far clipping plane.</summary>
	public double FarPlaneDistance
	{
		get => (double)GetValue(FarPlaneDistanceProperty)!;
		set => SetValue(FarPlaneDistanceProperty, value);
	}

	internal override Matrix3D ViewCore()
	{
		var z = -LookDirection;
		z.Normalize();
		var x = Vector3D.CrossProduct(UpDirection, z);
		x.Normalize();
		var y = Vector3D.CrossProduct(z, x);
		var p = (Vector3D)Position;
		return new Matrix3D(
			x.X, y.X, z.X, 0,
			x.Y, y.Y, z.Y, 0,
			x.Z, y.Z, z.Z, 0,
			-Vector3D.DotProduct(x, p), -Vector3D.DotProduct(y, p), -Vector3D.DotProduct(z, p), 1);
	}
}

/// <summary>A perspective camera; the field of view is horizontal.</summary>
public sealed class PerspectiveCamera : ProjectionCamera
{
	/// <summary>Identifies <see cref="FieldOfView"/>.</summary>
	public static readonly DependencyProperty FieldOfViewProperty =
		Media3DProps.Register(nameof(FieldOfView), typeof(double), typeof(PerspectiveCamera), 45.0);

	/// <summary>Creates a camera at the origin looking down -Z.</summary>
	public PerspectiveCamera() { }

	/// <summary>Creates a camera.</summary>
	public PerspectiveCamera(Point3D position, Vector3D lookDirection, Vector3D upDirection, double fieldOfView)
	{
		Position = position;
		LookDirection = lookDirection;
		UpDirection = upDirection;
		FieldOfView = fieldOfView;
	}

	/// <summary>The horizontal field of view in degrees.</summary>
	public double FieldOfView
	{
		get => (double)GetValue(FieldOfViewProperty)!;
		set => SetValue(FieldOfViewProperty, value);
	}

	internal override Matrix3D GetProjectionMatrix(double aspectRatio)
	{
		double zn = NearPlaneDistance, zf = FarPlaneDistance;
		double xScale = 1.0 / Math.Tan(FieldOfView * Math.PI / 360.0);
		double yScale = aspectRatio * xScale;
		double m33 = double.IsInfinity(zf) ? -1 : zf / (zn - zf);
		return new Matrix3D(xScale, 0, 0, 0, 0, yScale, 0, 0, 0, 0, m33, -1, 0, 0, zn * m33, 0);
	}

	/// <summary>Creates a modifiable clone.</summary>
	public new PerspectiveCamera Clone() => (PerspectiveCamera)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new PerspectiveCamera();
}

/// <summary>An orthographic camera.</summary>
public sealed class OrthographicCamera : ProjectionCamera
{
	/// <summary>Identifies <see cref="Width"/>.</summary>
	public static readonly DependencyProperty WidthProperty =
		Media3DProps.Register(nameof(Width), typeof(double), typeof(OrthographicCamera), 2.0);

	/// <summary>Creates a camera at the origin looking down -Z.</summary>
	public OrthographicCamera() { }

	/// <summary>Creates a camera.</summary>
	public OrthographicCamera(Point3D position, Vector3D lookDirection, Vector3D upDirection, double width)
	{
		Position = position;
		LookDirection = lookDirection;
		UpDirection = upDirection;
		Width = width;
	}

	/// <summary>The width of the view volume.</summary>
	public double Width
	{
		get => (double)GetValue(WidthProperty)!;
		set => SetValue(WidthProperty, value);
	}

	internal override Matrix3D GetProjectionMatrix(double aspectRatio)
	{
		double zn = NearPlaneDistance, zf = double.IsInfinity(FarPlaneDistance) ? 1e12 : FarPlaneDistance;
		double w = Width;
		double m33 = 1.0 / (zn - zf);
		return new Matrix3D(2.0 / w, 0, 0, 0, 0, 2.0 * aspectRatio / w, 0, 0, 0, 0, m33, 0, 0, 0, zn * m33, 1);
	}

	/// <summary>Creates a modifiable clone.</summary>
	public new OrthographicCamera Clone() => (OrthographicCamera)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new OrthographicCamera();
}
