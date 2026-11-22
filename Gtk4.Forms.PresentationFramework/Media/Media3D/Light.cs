namespace System.Windows.Media.Media3D;

/// <summary>Base of the lights.</summary>
public abstract class Light : Model3D
{
	/// <summary>Identifies <see cref="Color"/>.</summary>
	public static readonly DependencyProperty ColorProperty =
		Media3DProps.Register(nameof(Color), typeof(Color), typeof(Light), Colors.White);

	internal Light() { }

	/// <summary>The colour of the light.</summary>
	public Color Color
	{
		get => (Color)GetValue(ColorProperty)!;
		set => SetValue(ColorProperty, value);
	}

	internal override Rect3D BoundsCore => Rect3D.Empty;
}

/// <summary>Light that reaches every surface equally.</summary>
public sealed class AmbientLight : Light
{
	/// <summary>Creates a white ambient light.</summary>
	public AmbientLight() { }

	/// <summary>Creates an ambient light.</summary>
	public AmbientLight(Color ambientColor) => Color = ambientColor;

	/// <summary>Creates a modifiable clone.</summary>
	public new AmbientLight Clone() => (AmbientLight)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new AmbientLight();
}

/// <summary>Light from infinitely far away in one direction.</summary>
public sealed class DirectionalLight : Light
{
	/// <summary>Identifies <see cref="Direction"/>.</summary>
	public static readonly DependencyProperty DirectionProperty = Media3DProps.Register(
		nameof(Direction), typeof(Vector3D), typeof(DirectionalLight), new Vector3D(0, 0, -1));

	/// <summary>Creates a white light pointing down -Z.</summary>
	public DirectionalLight() { }

	/// <summary>Creates a directional light.</summary>
	public DirectionalLight(Color diffuseColor, Vector3D direction)
	{
		Color = diffuseColor;
		Direction = direction;
	}

	/// <summary>The direction the light travels.</summary>
	public Vector3D Direction
	{
		get => (Vector3D)GetValue(DirectionProperty)!;
		set => SetValue(DirectionProperty, value);
	}

	/// <summary>Creates a modifiable clone.</summary>
	public new DirectionalLight Clone() => (DirectionalLight)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new DirectionalLight();
}
