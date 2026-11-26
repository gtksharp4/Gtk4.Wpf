using System.Windows.Markup;

namespace System.Windows.Media.Media3D;

/// <summary>A material with a specular highlight.</summary>
public sealed class SpecularMaterial : Material
{
	/// <summary>Identifies <see cref="Brush"/>.</summary>
	public static readonly DependencyProperty BrushProperty =
		Media3DProps.Register(nameof(Brush), typeof(Brush), typeof(SpecularMaterial), null);

	/// <summary>Identifies <see cref="Color"/>.</summary>
	public static readonly DependencyProperty ColorProperty =
		Media3DProps.Register(nameof(Color), typeof(Color), typeof(SpecularMaterial), Colors.White);

	/// <summary>Identifies <see cref="SpecularPower"/>.</summary>
	public static readonly DependencyProperty SpecularPowerProperty =
		Media3DProps.Register(nameof(SpecularPower), typeof(double), typeof(SpecularMaterial), 40.0);

	/// <summary>Creates a material without a brush.</summary>
	public SpecularMaterial() { }

	/// <summary>Creates a material with a brush and specular power.</summary>
	public SpecularMaterial(Brush brush, double specularPower)
	{
		Brush = brush;
		SpecularPower = specularPower;
	}

	/// <summary>The brush.</summary>
	public Brush? Brush
	{
		get => (Brush?)GetValue(BrushProperty);
		set => SetValue(BrushProperty, value);
	}

	/// <summary>The highlight colour modulation.</summary>
	public Color Color
	{
		get => (Color)GetValue(ColorProperty)!;
		set => SetValue(ColorProperty, value);
	}

	/// <summary>The sharpness of the highlight.</summary>
	public double SpecularPower
	{
		get => (double)GetValue(SpecularPowerProperty)!;
		set => SetValue(SpecularPowerProperty, value);
	}

	/// <summary>Creates a modifiable clone.</summary>
	public new SpecularMaterial Clone() => (SpecularMaterial)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new SpecularMaterial();

	/// <inheritdoc/>
	protected override void OnFreeze() => Media3DProps.FreezeAll(Brush);
}

/// <summary>Layers several materials.</summary>
[ContentProperty("Children")]
public sealed class MaterialGroup : Material
{
	/// <summary>Identifies <see cref="Children"/>.</summary>
	public static readonly DependencyProperty ChildrenProperty = Media3DProps.Register(
		nameof(Children), typeof(MaterialCollection), typeof(MaterialGroup), MaterialCollection.Empty);

	/// <summary>Creates an empty group.</summary>
	public MaterialGroup() { }

	/// <summary>The child materials.</summary>
	public MaterialCollection Children
	{
		get => Media3DProps.Collection(this, ChildrenProperty, MaterialCollection.Empty);
		set => SetValue(ChildrenProperty, value);
	}

	/// <summary>Creates a modifiable clone.</summary>
	public new MaterialGroup Clone() => (MaterialGroup)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new MaterialGroup();

	/// <inheritdoc/>
	protected override void OnFreeze() =>
		Media3DProps.FreezeAll(Media3DProps.Peek<MaterialCollection>(this, ChildrenProperty));
}
