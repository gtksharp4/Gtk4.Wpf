namespace System.Windows.Media.Animation;

/// <summary>An object whose dependency properties can be animated.</summary>
public interface IAnimatable
{
	/// <summary>Whether any property currently has animation clocks applied.</summary>
	bool HasAnimatedProperties { get; }

	/// <summary>Animates <paramref name="dp"/>; a null timeline removes its animations.</summary>
	void BeginAnimation(DependencyProperty dp, AnimationTimeline? animation);

	/// <summary>Animates <paramref name="dp"/> with the given hand-off behaviour.</summary>
	void BeginAnimation(DependencyProperty dp, AnimationTimeline? animation, HandoffBehavior handoffBehavior);

	/// <summary>Applies an existing clock to <paramref name="dp"/>.</summary>
	void ApplyAnimationClock(DependencyProperty dp, AnimationClock? clock);

	/// <summary>Applies an existing clock to <paramref name="dp"/> with the given hand-off behaviour.</summary>
	void ApplyAnimationClock(DependencyProperty dp, AnimationClock? clock, HandoffBehavior handoffBehavior);

	/// <summary>Gets the value of <paramref name="dp"/> ignoring its animations.</summary>
	object? GetAnimationBaseValue(DependencyProperty dp);
}
