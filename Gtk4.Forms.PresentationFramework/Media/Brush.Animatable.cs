using System.Windows.Media.Animation;

namespace System.Windows.Media;

/// <summary>Makes every brush an <see cref="Animatable"/>, as in WPF.</summary>
public abstract partial class Brush : Animatable
{
	/// <summary>
	/// Creates a new instance of the same type through its parameterless constructor.
	/// Brushes with a richer constructor override this.
	/// </summary>
	protected override Freezable CreateInstanceCore() =>
		(Freezable)(Activator.CreateInstance(GetType()) ?? throw new NotSupportedException(GetType().Name));

	/// <summary>Identifies <see cref="Transform"/>. Default <see cref="Media.Transform.Identity"/>.</summary>
	public static readonly DependencyProperty TransformProperty = DependencyProperty.Register(
		nameof(Transform), typeof(Transform), typeof(Brush), new PropertyMetadata(Media.Transform.Identity));

	/// <summary>Identifies <see cref="RelativeTransform"/>. Default <see cref="Media.Transform.Identity"/>.</summary>
	public static readonly DependencyProperty RelativeTransformProperty = DependencyProperty.Register(
		nameof(RelativeTransform), typeof(Transform), typeof(Brush), new PropertyMetadata(Media.Transform.Identity));

	/// <summary>
	/// Transform applied to the brush output. Stored and animatable; the Cairo paint path does not
	/// apply it yet.
	/// </summary>
	public Transform Transform
	{
		get => (Transform)GetValue(TransformProperty)!;
		set => SetValue(TransformProperty, value);
	}

	/// <summary>Transform applied relative to the painted bounds. Stored and animatable; not applied when painting.</summary>
	public Transform RelativeTransform
	{
		get => (Transform)GetValue(RelativeTransformProperty)!;
		set => SetValue(RelativeTransformProperty, value);
	}
}
