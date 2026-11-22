namespace System.Windows.Media.Animation;

/// <summary>Abstract base for animations that produce <see cref="Thickness"/> values.</summary>
public abstract class ThicknessAnimationBase : AnimationTimeline
{
	/// <inheritdoc/>
	public sealed override Type TargetPropertyType => typeof(Thickness);

	/// <inheritdoc/>
	public sealed override object? GetCurrentValue(
		object? defaultOriginValue,
		object? defaultDestinationValue,
		AnimationClock animationClock)
	{
		if (defaultOriginValue == null) throw new ArgumentNullException(nameof(defaultOriginValue));
		if (defaultDestinationValue == null) throw new ArgumentNullException(nameof(defaultDestinationValue));
		return GetCurrentValue((Thickness)defaultOriginValue, (Thickness)defaultDestinationValue, animationClock);
	}

	/// <summary>Computes the animated value.</summary>
	public Thickness GetCurrentValue(Thickness defaultOriginValue, Thickness defaultDestinationValue, AnimationClock animationClock)
	{
		if (animationClock == null) throw new ArgumentNullException(nameof(animationClock));
		if (animationClock.CurrentState == ClockState.Stopped) return defaultDestinationValue;
		return GetCurrentValueCore(defaultOriginValue, defaultDestinationValue, animationClock);
	}

	/// <summary>Override to compute the animated value for the clock's current progress.</summary>
	protected abstract Thickness GetCurrentValueCore(
		Thickness defaultOriginValue, Thickness defaultDestinationValue, AnimationClock animationClock);
}
