namespace System.Windows.Media.Animation;

/// <summary>
/// Animates a <see cref="Point"/> property between <see cref="From"/> and <see cref="To"/> (or by
/// <see cref="By"/>) with linear interpolation of both coordinates.
/// </summary>
public class PointAnimation : PointAnimationBase
{
	/// <summary>Identifies <see cref="From"/>.</summary>
	public static readonly DependencyProperty FromProperty = DependencyProperty.Register(
		nameof(From), typeof(Point?), typeof(PointAnimation), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="To"/>.</summary>
	public static readonly DependencyProperty ToProperty = DependencyProperty.Register(
		nameof(To), typeof(Point?), typeof(PointAnimation), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="By"/>.</summary>
	public static readonly DependencyProperty ByProperty = DependencyProperty.Register(
		nameof(By), typeof(Point?), typeof(PointAnimation), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="EasingFunction"/>.</summary>
	public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register(
		nameof(EasingFunction), typeof(IEasingFunction), typeof(PointAnimation), new PropertyMetadata(null));

	/// <summary>Creates an animation with no endpoints set.</summary>
	public PointAnimation() { }

	/// <summary>Creates an animation to <paramref name="toValue"/>.</summary>
	public PointAnimation(Point toValue, Duration duration)
	{
		To = toValue;
		Duration = duration;
	}

	/// <summary>Creates an animation from <paramref name="fromValue"/> to <paramref name="toValue"/>.</summary>
	public PointAnimation(Point fromValue, Point toValue, Duration duration)
		: this(toValue, duration) => From = fromValue;

	/// <summary>Starting point; null uses the property value.</summary>
	public Point? From
	{
		get => (Point?)GetValue(FromProperty);
		set => SetValue(FromProperty, value);
	}

	/// <summary>Ending point; null uses <see cref="By"/> or the property value.</summary>
	public Point? To
	{
		get => (Point?)GetValue(ToProperty);
		set => SetValue(ToProperty, value);
	}

	/// <summary>Offset from the start point, used when <see cref="To"/> is null.</summary>
	public Point? By
	{
		get => (Point?)GetValue(ByProperty);
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
	protected override Freezable CreateInstanceCore() => new PointAnimation();

	/// <inheritdoc/>
	protected override Point GetCurrentValueCore(
		Point defaultOriginValue, Point defaultDestinationValue, AnimationClock animationClock)
	{
		AnimationEndpoints.Resolve(From, To, By, defaultOriginValue, defaultDestinationValue, Add,
			out var from, out var to);
		var t = AnimationEndpoints.Progress(animationClock, EasingFunction);
		return new Point(from.X + (to.X - from.X) * t, from.Y + (to.Y - from.Y) * t);
	}

	static Point Add(Point a, Point b) => new Point(a.X + b.X, a.Y + b.Y);
}
