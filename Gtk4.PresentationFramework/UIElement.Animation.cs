using System.Windows.Media.Animation;

namespace System.Windows;

/// <summary>
/// <see cref="IAnimatable"/> for <see cref="UIElement"/>: animated values sit above local values and
/// are removed (restoring the base value) by a null animation or a stopped clock.
/// </summary>
public abstract partial class UIElement : IAnimatable
{
	/// <inheritdoc/>
	public bool HasAnimatedProperties => AnimationStore.HasAnimatedProperties(this);

	/// <inheritdoc/>
	public void BeginAnimation(DependencyProperty dp, AnimationTimeline? animation) =>
		BeginAnimation(dp, animation, HandoffBehavior.SnapshotAndReplace);

	/// <inheritdoc/>
	public void BeginAnimation(DependencyProperty dp, AnimationTimeline? animation, HandoffBehavior handoffBehavior) =>
		AnimationStore.BeginAnimation(this, dp, animation, handoffBehavior);

	/// <inheritdoc/>
	public void ApplyAnimationClock(DependencyProperty dp, AnimationClock? clock) =>
		ApplyAnimationClock(dp, clock, HandoffBehavior.SnapshotAndReplace);

	/// <inheritdoc/>
	public void ApplyAnimationClock(DependencyProperty dp, AnimationClock? clock, HandoffBehavior handoffBehavior) =>
		AnimationStore.ApplyClock(this, dp, clock, handoffBehavior);

	/// <inheritdoc/>
	public object? GetAnimationBaseValue(DependencyProperty dp)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		return ResolveBaseValue(dp);
	}
}
