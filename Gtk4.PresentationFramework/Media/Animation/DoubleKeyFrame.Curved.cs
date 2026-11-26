namespace System.Windows.Media.Animation;

/// <summary>Interpolates along a <see cref="Animation.KeySpline"/>.</summary>
public class SplineDoubleKeyFrame : DoubleKeyFrame
{
	static KeySpline FrozenSpline()
	{
		var spline = new KeySpline(0, 0, 1, 1);
		spline.Freeze();
		return spline;
	}

	/// <summary>Identifies <see cref="KeySpline"/>.</summary>
	public static readonly DependencyProperty KeySplineProperty = DependencyProperty.Register(
		nameof(KeySpline), typeof(KeySpline), typeof(SplineDoubleKeyFrame), new PropertyMetadata(FrozenSpline()));

	/// <summary>Creates a frame.</summary>
	public SplineDoubleKeyFrame() { }

	/// <summary>Creates a frame.</summary>
	public SplineDoubleKeyFrame(double value) : base(value) { }

	/// <summary>Creates a frame.</summary>
	public SplineDoubleKeyFrame(double value, KeyTime keyTime) : base(value, keyTime) { }

	/// <summary>Creates a frame.</summary>
	public SplineDoubleKeyFrame(double value, KeyTime keyTime, KeySpline keySpline) : base(value, keyTime) =>
		KeySpline = keySpline;

	/// <summary>The easing spline; a frozen linear spline until set.</summary>
	public KeySpline KeySpline
	{
		get => (KeySpline)GetValue(KeySplineProperty)!;
		set
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			SetValue(KeySplineProperty, value);
		}
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new SplineDoubleKeyFrame();

	/// <inheritdoc/>
	protected override double InterpolateValueCore(double baseValue, double keyFrameProgress)
	{
		double eased = ((KeySpline?)GetValue(KeySplineProperty))?.GetSplineProgress(keyFrameProgress)
			?? keyFrameProgress;
		return baseValue + (Value - baseValue) * eased;
	}
}

/// <summary>Interpolates with an <see cref="IEasingFunction"/>.</summary>
public class EasingDoubleKeyFrame : DoubleKeyFrame
{
	/// <summary>Identifies <see cref="EasingFunction"/>.</summary>
	public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register(
		nameof(EasingFunction), typeof(IEasingFunction), typeof(EasingDoubleKeyFrame), new PropertyMetadata(null));

	/// <summary>Creates a frame.</summary>
	public EasingDoubleKeyFrame() { }

	/// <summary>Creates a frame.</summary>
	public EasingDoubleKeyFrame(double value) : base(value) { }

	/// <summary>Creates a frame.</summary>
	public EasingDoubleKeyFrame(double value, KeyTime keyTime) : base(value, keyTime) { }

	/// <summary>Creates a frame.</summary>
	public EasingDoubleKeyFrame(double value, KeyTime keyTime, IEasingFunction easingFunction)
		: base(value, keyTime) => EasingFunction = easingFunction;

	/// <summary>The easing applied to the frame progress.</summary>
	public IEasingFunction? EasingFunction
	{
		get => (IEasingFunction?)GetValue(EasingFunctionProperty);
		set => SetValue(EasingFunctionProperty, value);
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new EasingDoubleKeyFrame();

	/// <inheritdoc/>
	protected override double InterpolateValueCore(double baseValue, double keyFrameProgress)
	{
		var easing = EasingFunction;
		double eased = easing == null ? keyFrameProgress : easing.Ease(keyFrameProgress);
		return baseValue + (Value - baseValue) * eased;
	}
}
