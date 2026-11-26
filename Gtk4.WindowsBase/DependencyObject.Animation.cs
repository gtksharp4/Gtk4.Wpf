using System;
using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Animation-value layer for <see cref="DependencyObject"/>.
/// Priority: animation > local > trigger > style > inherited > default.
/// Animations write through <see cref="SetAnimationValue"/> / <see cref="ClearAnimationValue"/>
/// and sit ABOVE local in the resolution chain.
/// </summary>
public partial class DependencyObject
{
	// Animation value store: keyed by DP, one value per animator clock (last-writer wins).
	// A null entry is valid (animation to null/default).
	readonly Dictionary<DependencyProperty, object?> _animationValues =
		new Dictionary<DependencyProperty, object?>();

	/// <summary>
	/// Sets an animation-layer value (raw; coercion is applied on top). This is the highest-priority
	/// layer, sitting above the local value. Fires change notifications when the effective value
	/// differs from the previous effective value.
	/// </summary>
	internal void SetAnimationValue(DependencyProperty dp, object? value)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));

		var oldEffective = ResolveEffectiveValue(dp);
		_animationValues[dp] = value;
		CommitLayerChange(dp, oldEffective);
	}

	/// <summary>
	/// Removes the animation-layer value for <paramref name="dp"/>.
	/// The effective value reverts to local/trigger/style/inherited/default.
	/// </summary>
	internal void ClearAnimationValue(DependencyProperty dp)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		if (!_animationValues.ContainsKey(dp)) return;

		var oldEffective = ResolveEffectiveValue(dp);
		_animationValues.Remove(dp);
		CommitLayerChange(dp, oldEffective);
	}

	/// <summary>
	/// Returns <c>true</c> when an animation value is currently active for
	/// <paramref name="dp"/>.
	/// </summary>
	internal bool HasAnimationValue(DependencyProperty dp) =>
		_animationValues.ContainsKey(dp);
}
