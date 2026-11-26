namespace System.Windows.Media.Animation;

/// <summary>Abstract base for animations that produce <see cref="Point"/> values.</summary>
public abstract class PointAnimationBase : AnimationTimeline
{
	/// <inheritdoc/>
	public sealed override Type TargetPropertyType => typeof(Point);

	/// <inheritdoc/>
	public sealed override object? GetCurrentValue(
		object? defaultOriginValue,
		object? defaultDestinationValue,
		AnimationClock animationClock)
	{
		if (defaultOriginValue == null) throw new ArgumentNullException(nameof(defaultOriginValue));
		if (defaultDestinationValue == null) throw new ArgumentNullException(nameof(defaultDestinationValue));
		return GetCurrentValue((Point)defaultOriginValue, (Point)defaultDestinationValue, animationClock);
	}

	/// <summary>Computes the animated value.</summary>
	public Point GetCurrentValue(Point defaultOriginValue, Point defaultDestinationValue, AnimationClock animationClock)
	{
		if (animationClock == null) throw new ArgumentNullException(nameof(animationClock));
		if (animationClock.CurrentState == ClockState.Stopped) return defaultDestinationValue;
		return GetCurrentValueCore(defaultOriginValue, defaultDestinationValue, animationClock);
	}

	/// <summary>Override to compute the animated value for the clock's current progress.</summary>
	protected abstract Point GetCurrentValueCore(
		Point defaultOriginValue, Point defaultDestinationValue, AnimationClock animationClock);
}
