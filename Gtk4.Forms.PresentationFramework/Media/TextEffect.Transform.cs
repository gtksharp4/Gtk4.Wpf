namespace System.Windows.Media;

public sealed partial class TextEffect
{
	/// <summary>Identifies the Transform property (default the identity transform).</summary>
	public static readonly DependencyProperty TransformProperty = DependencyProperty.Register(
		nameof(Transform), typeof(Transform), typeof(TextEffect), new PropertyMetadata(Media.Transform.Identity));

	/// <summary>Identifies the Clip property (default null).</summary>
	public static readonly DependencyProperty ClipProperty = DependencyProperty.Register(
		nameof(Clip), typeof(Geometry), typeof(TextEffect), new PropertyMetadata(null));

	/// <summary>Creates an effect with its foreground, transform and character range.</summary>
	public TextEffect(Brush? foreground, Transform? transform, int positionStart, int positionCount)
	{
		Foreground = foreground;
		Transform = transform;
		PositionStart = positionStart;
		PositionCount = positionCount;
	}

	/// <summary>Creates an effect with the defaults.</summary>
	public TextEffect()
	{
	}

	/// <summary>Gets or sets the transform of the affected characters. Stored only; the label ignores it.</summary>
	public Transform? Transform
	{
		get => (Transform?)GetValue(TransformProperty);
		set => SetValue(TransformProperty, value);
	}

	/// <summary>Gets or sets the clip of the affected characters. Stored only; the label ignores it.</summary>
	public Geometry? Clip
	{
		get => (Geometry?)GetValue(ClipProperty);
		set => SetValue(ClipProperty, value);
	}
}
