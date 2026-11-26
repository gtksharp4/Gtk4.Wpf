namespace System.Windows.Media.Media3D;

/// <summary>Base of the lights with a position.</summary>
public abstract class PointLightBase : Light
{
	/// <summary>Identifies <see cref="Position"/>.</summary>
	public static readonly DependencyProperty PositionProperty = Media3DProps.Register(
		nameof(Position), typeof(Point3D), typeof(PointLightBase), new Point3D(0, 0, 0));

	/// <summary>Identifies <see cref="Range"/>.</summary>
	public static readonly DependencyProperty RangeProperty = Media3DProps.Register(
		nameof(Range), typeof(double), typeof(PointLightBase), double.PositiveInfinity);

	/// <summary>Identifies <see cref="ConstantAttenuation"/>.</summary>
	public static readonly DependencyProperty ConstantAttenuationProperty = Media3DProps.Register(
		nameof(ConstantAttenuation), typeof(double), typeof(PointLightBase), 1.0);

	/// <summary>Identifies <see cref="LinearAttenuation"/>.</summary>
	public static readonly DependencyProperty LinearAttenuationProperty = Media3DProps.Register(
		nameof(LinearAttenuation), typeof(double), typeof(PointLightBase), 0.0);

	/// <summary>Identifies <see cref="QuadraticAttenuation"/>.</summary>
	public static readonly DependencyProperty QuadraticAttenuationProperty = Media3DProps.Register(
		nameof(QuadraticAttenuation), typeof(double), typeof(PointLightBase), 0.0);

	internal PointLightBase() { }

	/// <summary>The position of the light.</summary>
	public Point3D Position
	{
		get => (Point3D)GetValue(PositionProperty)!;
		set => SetValue(PositionProperty, value);
	}

	/// <summary>The distance beyond which the light has no effect.</summary>
	public double Range
	{
		get => (double)GetValue(RangeProperty)!;
		set => SetValue(RangeProperty, value);
	}

	/// <summary>The constant attenuation term.</summary>
	public double ConstantAttenuation
	{
		get => (double)GetValue(ConstantAttenuationProperty)!;
		set => SetValue(ConstantAttenuationProperty, value);
	}

	/// <summary>The linear attenuation term.</summary>
	public double LinearAttenuation
	{
		get => (double)GetValue(LinearAttenuationProperty)!;
		set => SetValue(LinearAttenuationProperty, value);
	}

	/// <summary>The quadratic attenuation term.</summary>
	public double QuadraticAttenuation
	{
		get => (double)GetValue(QuadraticAttenuationProperty)!;
		set => SetValue(QuadraticAttenuationProperty, value);
	}
}

/// <summary>A light radiating from a point.</summary>
public sealed class PointLight : PointLightBase
{
	/// <summary>Creates a white light at the origin.</summary>
	public PointLight() { }

	/// <summary>Creates a point light.</summary>
	public PointLight(Color diffuseColor, Point3D position)
	{
		Color = diffuseColor;
		Position = position;
	}

	/// <summary>Creates a modifiable clone.</summary>
	public new PointLight Clone() => (PointLight)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new PointLight();
}

/// <summary>A cone of light.</summary>
public sealed class SpotLight : PointLightBase
{
	/// <summary>Identifies <see cref="Direction"/>.</summary>
	public static readonly DependencyProperty DirectionProperty = Media3DProps.Register(
		nameof(Direction), typeof(Vector3D), typeof(SpotLight), new Vector3D(0, 0, -1));

	/// <summary>Identifies <see cref="OuterConeAngle"/>.</summary>
	public static readonly DependencyProperty OuterConeAngleProperty =
		Media3DProps.Register(nameof(OuterConeAngle), typeof(double), typeof(SpotLight), 90.0);

	/// <summary>Identifies <see cref="InnerConeAngle"/>.</summary>
	public static readonly DependencyProperty InnerConeAngleProperty =
		Media3DProps.Register(nameof(InnerConeAngle), typeof(double), typeof(SpotLight), 180.0);

	/// <summary>Creates a white spot light at the origin.</summary>
	public SpotLight() { }

	/// <summary>Creates a spot light.</summary>
	public SpotLight(Color diffuseColor, Point3D position, Vector3D direction, double outerConeAngle,
		double innerConeAngle)
	{
		Color = diffuseColor;
		Position = position;
		Direction = direction;
		OuterConeAngle = outerConeAngle;
		InnerConeAngle = innerConeAngle;
	}

	/// <summary>The direction of the cone axis.</summary>
	public Vector3D Direction
	{
		get => (Vector3D)GetValue(DirectionProperty)!;
		set => SetValue(DirectionProperty, value);
	}

	/// <summary>The angle in degrees outside of which there is no light.</summary>
	public double OuterConeAngle
	{
		get => (double)GetValue(OuterConeAngleProperty)!;
		set => SetValue(OuterConeAngleProperty, value);
	}

	/// <summary>The angle in degrees inside of which the light is at full strength.</summary>
	public double InnerConeAngle
	{
		get => (double)GetValue(InnerConeAngleProperty)!;
		set => SetValue(InnerConeAngleProperty, value);
	}

	/// <summary>Creates a modifiable clone.</summary>
	public new SpotLight Clone() => (SpotLight)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new SpotLight();
}
