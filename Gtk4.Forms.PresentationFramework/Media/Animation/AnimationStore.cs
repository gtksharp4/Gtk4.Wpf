using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Windows.Media.Animation;

/// <summary>
/// Per-object, per-property stacks of animation clocks. The stack is folded into one animated
/// value (written to the animation layer of the object) every time one of its clocks changes.
/// </summary>
internal static class AnimationStore
{
	static readonly ConditionalWeakTable<DependencyObject, Dictionary<DependencyProperty, List<AnimationClock>>>
		_stacks = new ConditionalWeakTable<DependencyObject, Dictionary<DependencyProperty, List<AnimationClock>>>();

	internal static bool HasAnimatedProperties(DependencyObject target) =>
		_stacks.TryGetValue(target, out var map) && map.Count > 0;

	internal static void BeginAnimation(
		DependencyObject target, DependencyProperty dp, AnimationTimeline? timeline, HandoffBehavior handoff)
	{
		Validate(dp, handoff);
		if (timeline == null)
		{
			RemoveAll(target, dp);
			return;
		}
		Apply(target, dp, timeline.CreateClock(), handoff);
	}

	internal static void ApplyClock(
		DependencyObject target, DependencyProperty dp, AnimationClock? clock, HandoffBehavior handoff)
	{
		Validate(dp, handoff);
		if (clock == null)
		{
			RemoveAll(target, dp);
			return;
		}
		Apply(target, dp, clock, handoff);
	}

	static void Validate(DependencyProperty dp, HandoffBehavior handoff)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		if (handoff != HandoffBehavior.SnapshotAndReplace && handoff != HandoffBehavior.Compose)
			throw new ArgumentException("Invalid HandoffBehavior value.", nameof(handoff));
		if (dp.IsReadOnly)
			throw new InvalidOperationException($"Cannot animate the read-only property '{dp.Name}'.");
	}

	internal static void Apply(
		DependencyObject target, DependencyProperty dp, AnimationClock clock, HandoffBehavior handoff)
	{
		var type = clock.Timeline.TargetPropertyType;
		if (type != typeof(object) && !dp.PropertyType.IsAssignableFrom(type)
			&& !type.IsAssignableFrom(dp.PropertyType))
			throw new InvalidOperationException(
				$"Cannot animate '{dp.Name}' of type '{dp.PropertyType.Name}' with '{clock.Timeline.GetType().Name}' "
				+ $"(it animates '{type.Name}').");
		if (target is Freezable { IsFrozen: true })
			throw new InvalidOperationException("Cannot animate a frozen object.");

		var list = StackFor(target, dp);
		list.RemoveAll(c => c.CurrentState == ClockState.Stopped && !ReferenceEquals(c, clock));
		if (handoff == HandoffBehavior.SnapshotAndReplace)
		{
			if (list.Count > 0)
				clock.SetSnapshot(target.GetValue(dp));
			foreach (var old in list.ToArray())
				Detach(target, dp, old, refresh: false);
		}
		list = StackFor(target, dp);
		list.Add(clock);
		clock.AddAttachment(target, dp);
		AnimationScheduler.Register(clock.Root);
		Refresh(target, dp);
	}

	static List<AnimationClock> StackFor(DependencyObject target, DependencyProperty dp)
	{
		var map = _stacks.GetOrCreateValue(target);
		if (!map.TryGetValue(dp, out var list))
			map[dp] = list = new List<AnimationClock>();
		return list;
	}

	internal static void RemoveAll(DependencyObject target, DependencyProperty dp)
	{
		if (!_stacks.TryGetValue(target, out var map) || !map.TryGetValue(dp, out var list)) return;
		foreach (var clock in list.ToArray())
			Detach(target, dp, clock);
	}

	internal static void Detach(
		DependencyObject target, DependencyProperty dp, AnimationClock clock, bool refresh = true)
	{
		clock.RemoveAttachment(target, dp);
		if (_stacks.TryGetValue(target, out var map) && map.TryGetValue(dp, out var list))
		{
			list.Remove(clock);
			if (list.Count == 0) map.Remove(dp);
		}
		if (refresh) Refresh(target, dp);
	}

	/// <summary>Folds the clocks of one property into its animated value.</summary>
	internal static void Refresh(DependencyObject target, DependencyProperty dp)
	{
		var baseValue = target.ResolveBaseValue(dp);
		object? value = baseValue;
		var any = false;
		if (_stacks.TryGetValue(target, out var map) && map.TryGetValue(dp, out var list))
		{
			foreach (var clock in list)
			{
				if (!clock.HasValue) continue;
				var origin = clock.HasSnapshot ? clock.SnapshotValue : value;
				value = clock.GetCurrentValue(origin, baseValue);
				any = true;
			}
		}
		if (any) target.SetAnimationValue(dp, value);
		else target.ClearAnimationValue(dp);
	}

	/// <summary>Attaches a clock without hand-off or scheduling (storyboards and tests drive the clock).</summary>
	internal static void AttachClock(DependencyObject target, DependencyProperty dp, AnimationClock clock)
	{
		StackFor(target, dp).Add(clock);
		clock.AddAttachment(target, dp);
	}

	internal static void CopyAnimatedValues(DependencyObject source, Freezable destination)
	{
		if (!_stacks.TryGetValue(source, out var map)) return;
		foreach (var dp in map.Keys)
			destination.SetValue(dp, source.GetValue(dp));
	}
}
