namespace System.Windows.Media.Animation;

/// <summary>
/// A <see cref="Freezable"/> whose dependency properties can be animated.
/// Mirrors <c>System.Windows.Media.Animation.Animatable</c>.
/// </summary>
public abstract class Animatable : Freezable, IAnimatable
{
	/// <inheritdoc/>
	public bool HasAnimatedProperties => AnimationStore.HasAnimatedProperties(this);

	/// <inheritdoc/>
	public void BeginAnimation(DependencyProperty dp, AnimationTimeline? animation) =>
		BeginAnimation(dp, animation, HandoffBehavior.SnapshotAndReplace);

	/// <inheritdoc/>
	public void BeginAnimation(DependencyProperty dp, AnimationTimeline? animation, HandoffBehavior handoffBehavior)
	{
		ThrowIfFrozen();
		AnimationStore.BeginAnimation(this, dp, animation, handoffBehavior);
	}

	/// <inheritdoc/>
	public void ApplyAnimationClock(DependencyProperty dp, AnimationClock? clock) =>
		ApplyAnimationClock(dp, clock, HandoffBehavior.SnapshotAndReplace);

	/// <inheritdoc/>
	public void ApplyAnimationClock(DependencyProperty dp, AnimationClock? clock, HandoffBehavior handoffBehavior)
	{
		ThrowIfFrozen();
		AnimationStore.ApplyClock(this, dp, clock, handoffBehavior);
	}

	/// <inheritdoc/>
	public object? GetAnimationBaseValue(DependencyProperty dp)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		return ResolveBaseValue(dp);
	}

	/// <summary>Copies the animated values when cloning the current value.</summary>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		if (currentValueOnly)
			AnimationStore.CopyAnimatedValues(this, destination);
	}
}
