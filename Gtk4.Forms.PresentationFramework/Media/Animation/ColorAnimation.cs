namespace System.Windows.Media.Animation;

/// <summary>
/// Animates a <see cref="Color"/> property between <see cref="From"/> and <see cref="To"/> (or by
/// <see cref="By"/>) with per-channel linear interpolation.
/// </summary>
public class ColorAnimation : ColorAnimationBase
{
	/// <summary>Identifies <see cref="From"/>.</summary>
	public static readonly DependencyProperty FromProperty = DependencyProperty.Register(
		nameof(From), typeof(Color?), typeof(ColorAnimation), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="To"/>.</summary>
	public static readonly DependencyProperty ToProperty = DependencyProperty.Register(
		nameof(To), typeof(Color?), typeof(ColorAnimation), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="By"/>.</summary>
	public static readonly DependencyProperty ByProperty = DependencyProperty.Register(
		nameof(By), typeof(Color?), typeof(ColorAnimation), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="EasingFunction"/>.</summary>
	public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register(
		nameof(EasingFunction), typeof(IEasingFunction), typeof(ColorAnimation), new PropertyMetadata(null));

	/// <summary>Creates an animation with no endpoints set.</summary>
	public ColorAnimation() { }

	/// <summary>Creates an animation to <paramref name="toValue"/>.</summary>
	public ColorAnimation(Color toValue, Duration duration)
	{
		To = toValue;
		Duration = duration;
	}

	/// <summary>Creates an animation from <paramref name="fromValue"/> to <paramref name="toValue"/>.</summary>
	public ColorAnimation(Color fromValue, Color toValue, Duration duration) : this(toValue, duration) =>
		From = fromValue;

	/// <summary>Starting colour; null uses the property value.</summary>
	public Color? From
	{
		get => (Color?)GetValue(FromProperty);
		set => SetValue(FromProperty, value);
	}

	/// <summary>Ending colour; null uses <see cref="By"/> or the property value.</summary>
	public Color? To
	{
		get => (Color?)GetValue(ToProperty);
		set => SetValue(ToProperty, value);
	}

	/// <summary>Per-channel offset from the start colour, used when <see cref="To"/> is null.</summary>
	public Color? By
	{
		get => (Color?)GetValue(ByProperty);
		set => SetValue(ByProperty, value);
	}

	/// <summary>Easing applied to the linear progress.</summary>
	public IEasingFunction? EasingFunction
	{
		get => (IEasingFunction?)GetValue(EasingFunctionProperty);
		set => SetValue(EasingFunctionProperty, value);
	}

	/// <inheritdoc/>
	public override bool IsDestinationDefault => To == null && By == null;

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new ColorAnimation();

	/// <inheritdoc/>
	protected override Color GetCurrentValueCore(
		Color defaultOriginValue, Color defaultDestinationValue, AnimationClock animationClock)
	{
		AnimationEndpoints.Resolve(From, To, By, defaultOriginValue, defaultDestinationValue, Add,
			out var from, out var to);
		var t = AnimationEndpoints.Progress(animationClock, EasingFunction);
		return Color.FromArgb(Lerp(from.A, to.A, t), Lerp(from.R, to.R, t), Lerp(from.G, to.G, t),
			Lerp(from.B, to.B, t));
	}

	static Color Add(Color a, Color b) => Color.FromArgb(
		(byte)Math.Min(255, a.A + b.A), (byte)Math.Min(255, a.R + b.R),
		(byte)Math.Min(255, a.G + b.G), (byte)Math.Min(255, a.B + b.B));

	static byte Lerp(byte a, byte b, double t) => (byte)Math.Round(Math.Max(0, Math.Min(255, a + (b - a) * t)));
}
