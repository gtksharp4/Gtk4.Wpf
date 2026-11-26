namespace System.Windows.Media.Animation;

/// <summary>
/// Animates a <see cref="Thickness"/> property between <see cref="From"/> and <see cref="To"/> (or by
/// <see cref="By"/>) with per-side linear interpolation.
/// </summary>
public class ThicknessAnimation : ThicknessAnimationBase
{
	/// <summary>Identifies <see cref="From"/>.</summary>
	public static readonly DependencyProperty FromProperty = DependencyProperty.Register(
		nameof(From), typeof(Thickness?), typeof(ThicknessAnimation), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="To"/>.</summary>
	public static readonly DependencyProperty ToProperty = DependencyProperty.Register(
		nameof(To), typeof(Thickness?), typeof(ThicknessAnimation), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="By"/>.</summary>
	public static readonly DependencyProperty ByProperty = DependencyProperty.Register(
		nameof(By), typeof(Thickness?), typeof(ThicknessAnimation), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="EasingFunction"/>.</summary>
	public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register(
		nameof(EasingFunction), typeof(IEasingFunction), typeof(ThicknessAnimation), new PropertyMetadata(null));

	/// <summary>Creates an animation with no endpoints set.</summary>
	public ThicknessAnimation() { }

	/// <summary>Creates an animation to <paramref name="toValue"/>.</summary>
	public ThicknessAnimation(Thickness toValue, Duration duration)
	{
		To = toValue;
		Duration = duration;
	}

	/// <summary>Creates an animation from <paramref name="fromValue"/> to <paramref name="toValue"/>.</summary>
	public ThicknessAnimation(Thickness fromValue, Thickness toValue, Duration duration)
		: this(toValue, duration) => From = fromValue;

	/// <summary>Starting thickness; null uses the property value.</summary>
	public Thickness? From
	{
		get => (Thickness?)GetValue(FromProperty);
		set => SetValue(FromProperty, value);
	}

	/// <summary>Ending thickness; null uses <see cref="By"/> or the property value.</summary>
	public Thickness? To
	{
		get => (Thickness?)GetValue(ToProperty);
		set => SetValue(ToProperty, value);
	}

	/// <summary>Per-side offset from the start thickness, used when <see cref="To"/> is null.</summary>
	public Thickness? By
	{
		get => (Thickness?)GetValue(ByProperty);
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
	protected override Freezable CreateInstanceCore() => new ThicknessAnimation();

	/// <inheritdoc/>
	protected override Thickness GetCurrentValueCore(
		Thickness defaultOriginValue, Thickness defaultDestinationValue, AnimationClock animationClock)
	{
		AnimationEndpoints.Resolve(From, To, By, defaultOriginValue, defaultDestinationValue, Add,
			out var from, out var to);
		var t = AnimationEndpoints.Progress(animationClock, EasingFunction);
		return new Thickness(Lerp(from.Left, to.Left, t), Lerp(from.Top, to.Top, t),
			Lerp(from.Right, to.Right, t), Lerp(from.Bottom, to.Bottom, t));
	}

	static Thickness Add(Thickness a, Thickness b) =>
		new Thickness(a.Left + b.Left, a.Top + b.Top, a.Right + b.Right, a.Bottom + b.Bottom);

	static double Lerp(double a, double b, double t) => a + (b - a) * t;
}
