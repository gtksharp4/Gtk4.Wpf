namespace System.Windows.Media.Animation;

/// <summary>
/// Base class for timelines that animate a property value.
/// Mirrors <c>System.Windows.Media.Animation.AnimationTimeline</c>.
/// </summary>
public abstract class AnimationTimeline : Timeline
{
	/// <summary>The type of the property this animation produces values for.</summary>
	public abstract Type TargetPropertyType { get; }

	/// <summary>Whether the destination value comes from the property rather than the animation.</summary>
	public virtual bool IsDestinationDefault => false;

	/// <summary>Creates a controllable animation clock.</summary>
	public new AnimationClock CreateClock() => (AnimationClock)base.CreateClock();

	/// <summary>Creates an animation clock; <paramref name="hasControllableRoot"/> gives it a controller.</summary>
	public new AnimationClock CreateClock(bool hasControllableRoot) =>
		(AnimationClock)base.CreateClock(hasControllableRoot);

	protected internal override Clock AllocateClock() => new AnimationClock(this);

	/// <summary>
	/// Computes the animated value for the clock's current progress; the base returns
	/// <paramref name="defaultDestinationValue"/>. <paramref name="defaultOriginValue"/> is the value the
	/// animation starts from by default and <paramref name="defaultDestinationValue"/> the one it ends at when
	/// no destination is set.
	/// </summary>
	public virtual object? GetCurrentValue(
		object? defaultOriginValue,
		object? defaultDestinationValue,
		AnimationClock animationClock) => defaultDestinationValue;
}
