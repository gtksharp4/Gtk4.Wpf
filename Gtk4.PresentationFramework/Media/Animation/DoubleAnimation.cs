namespace System.Windows.Media.Animation;

/// <summary>
/// Animates a <see cref="double"/> property from <see cref="From"/> to <see cref="To"/> (or by
/// <see cref="By"/>) with linear interpolation, optionally eased.
/// </summary>
public class DoubleAnimation : DoubleAnimationBase
{
	/// <summary>Identifies <see cref="From"/>.</summary>
	public static readonly DependencyProperty FromProperty = DependencyProperty.Register(
		nameof(From), typeof(double?), typeof(DoubleAnimation), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="To"/>.</summary>
	public static readonly DependencyProperty ToProperty = DependencyProperty.Register(
		nameof(To), typeof(double?), typeof(DoubleAnimation), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="By"/>.</summary>
	public static readonly DependencyProperty ByProperty = DependencyProperty.Register(
		nameof(By), typeof(double?), typeof(DoubleAnimation), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="EasingFunction"/>.</summary>
	public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register(
		nameof(EasingFunction), typeof(IEasingFunction), typeof(DoubleAnimation), new PropertyMetadata(null));

	/// <summary>Creates an animation with no endpoints set.</summary>
	public DoubleAnimation() { }

	/// <summary>Creates an animation to <paramref name="toValue"/>.</summary>
	public DoubleAnimation(double toValue, Duration duration) : this(toValue, duration, FillBehavior.HoldEnd) { }

	/// <summary>Creates an animation to <paramref name="toValue"/>.</summary>
	public DoubleAnimation(double toValue, Duration duration, FillBehavior fillBehavior)
	{
		To = toValue;
		Duration = duration;
		FillBehavior = fillBehavior;
	}

	/// <summary>Creates an animation from <paramref name="fromValue"/> to <paramref name="toValue"/>.</summary>
	public DoubleAnimation(double fromValue, double toValue, Duration duration)
		: this(fromValue, toValue, duration, FillBehavior.HoldEnd) { }

	/// <summary>Creates an animation from <paramref name="fromValue"/> to <paramref name="toValue"/>.</summary>
	public DoubleAnimation(double fromValue, double toValue, Duration duration, FillBehavior fillBehavior)
		: this(toValue, duration, fillBehavior) => From = fromValue;

	/// <summary>Starting value; null uses the property value.</summary>
	public double? From
	{
		get => (double?)GetValue(FromProperty);
		set => SetValue(FromProperty, value);
	}

	/// <summary>Ending value; null uses <see cref="By"/> or the property value.</summary>
	public double? To
	{
		get => (double?)GetValue(ToProperty);
		set => SetValue(ToProperty, value);
	}

	/// <summary>Offset from the start value, used when <see cref="To"/> is null.</summary>
	public double? By
	{
		get => (double?)GetValue(ByProperty);
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
	protected override Freezable CreateInstanceCore() => new DoubleAnimation();

	/// <inheritdoc/>
	protected override double GetCurrentValueCore(
		double defaultOriginValue, double defaultDestinationValue, AnimationClock animationClock)
	{
		AnimationEndpoints.Resolve(From, To, By, defaultOriginValue, defaultDestinationValue,
			(a, b) => a + b, out var from, out var to);
		return from + (to - from) * AnimationEndpoints.Progress(animationClock, EasingFunction);
	}
}
