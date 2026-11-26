using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace System.Windows.Media.Media3D;

/// <summary>Base of the surface materials.</summary>
public abstract class Material : Animatable
{
	ChildLinks? _links;

	internal Material() { }

	void RaiseChanged()
	{
		if (!IsFrozen) OnChanged();
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.NewValue is Freezable || e.OldValue is Freezable)
			(_links ??= new ChildLinks(RaiseChanged)).Track(e);
	}
}

/// <summary>A material lit by the scene lights.</summary>
public sealed class DiffuseMaterial : Material
{
	/// <summary>Identifies <see cref="Brush"/>.</summary>
	public static readonly DependencyProperty BrushProperty =
		Media3DProps.Register(nameof(Brush), typeof(Brush), typeof(DiffuseMaterial), null);

	/// <summary>Identifies <see cref="Color"/>.</summary>
	public static readonly DependencyProperty ColorProperty =
		Media3DProps.Register(nameof(Color), typeof(Color), typeof(DiffuseMaterial), Colors.White);

	/// <summary>Identifies <see cref="AmbientColor"/>.</summary>
	public static readonly DependencyProperty AmbientColorProperty =
		Media3DProps.Register(nameof(AmbientColor), typeof(Color), typeof(DiffuseMaterial), Colors.White);

	/// <summary>Creates a material without a brush.</summary>
	public DiffuseMaterial() { }

	/// <summary>Creates a material painted with <paramref name="brush"/>.</summary>
	public DiffuseMaterial(Brush brush) => Brush = brush;

	/// <summary>The brush.</summary>
	public Brush? Brush
	{
		get => (Brush?)GetValue(BrushProperty);
		set => SetValue(BrushProperty, value);
	}

	/// <summary>The colour that modulates the diffuse light.</summary>
	public Color Color
	{
		get => (Color)GetValue(ColorProperty)!;
		set => SetValue(ColorProperty, value);
	}

	/// <summary>The colour that modulates the ambient light.</summary>
	public Color AmbientColor
	{
		get => (Color)GetValue(AmbientColorProperty)!;
		set => SetValue(AmbientColorProperty, value);
	}

	/// <summary>Creates a modifiable clone.</summary>
	public new DiffuseMaterial Clone() => (DiffuseMaterial)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new DiffuseMaterial();

	/// <inheritdoc/>
	protected override void OnFreeze() => Media3DProps.FreezeAll(Brush);
}

/// <summary>A material that emits light and ignores scene lighting.</summary>
public sealed class EmissiveMaterial : Material
{
	/// <summary>Identifies <see cref="Brush"/>.</summary>
	public static readonly DependencyProperty BrushProperty =
		Media3DProps.Register(nameof(Brush), typeof(Brush), typeof(EmissiveMaterial), null);

	/// <summary>Identifies <see cref="Color"/>.</summary>
	public static readonly DependencyProperty ColorProperty =
		Media3DProps.Register(nameof(Color), typeof(Color), typeof(EmissiveMaterial), Colors.White);

	/// <summary>Creates a material without a brush.</summary>
	public EmissiveMaterial() { }

	/// <summary>Creates a material painted with <paramref name="brush"/>.</summary>
	public EmissiveMaterial(Brush brush) => Brush = brush;

	/// <summary>The brush.</summary>
	public Brush? Brush
	{
		get => (Brush?)GetValue(BrushProperty);
		set => SetValue(BrushProperty, value);
	}

	/// <summary>The emitted colour modulation.</summary>
	public Color Color
	{
		get => (Color)GetValue(ColorProperty)!;
		set => SetValue(ColorProperty, value);
	}

	/// <summary>Creates a modifiable clone.</summary>
	public new EmissiveMaterial Clone() => (EmissiveMaterial)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new EmissiveMaterial();

	/// <inheritdoc/>
	protected override void OnFreeze() => Media3DProps.FreezeAll(Brush);
}
