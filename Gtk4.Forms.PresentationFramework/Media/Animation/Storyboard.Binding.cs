using System.Collections.Generic;
using System.Diagnostics;

namespace System.Windows.Media.Animation;

public partial class Storyboard
{
	/// <summary>Applies every animation clock below <paramref name="group"/> to its target property.</summary>
	void BindClocks(
		ClockGroup group, FrameworkElement container, string? name, DependencyObject? target,
		PropertyPath? path, HandoffBehavior handoff, List<AnimationClock> leaves,
		HashSet<(DependencyObject, DependencyProperty)>? bound = null)
	{
		bound ??= new HashSet<(DependencyObject, DependencyProperty)>();
		for (var i = 0; i < group.Children.Count; i++)
		{
			var timeline = group.Timeline.Children[i];
			var childName = (string?)timeline.GetValue(TargetNameProperty) ?? name;
			var childTarget = (DependencyObject?)timeline.GetValue(TargetProperty) ?? target;
			var childPath = (PropertyPath?)timeline.GetValue(TargetPropertyProperty) ?? path;
			switch (group.Children[i])
			{
				case ClockGroup nested:
					BindClocks(nested, container, childName, childTarget, childPath, handoff, leaves, bound);
					break;
				case MediaClock media:
					BindMedia(media, container, childName, childTarget);
					break;
				case AnimationClock clock:
					(DependencyObject, DependencyProperty) resolved;
					try { resolved = ResolveTarget(container, childName, childTarget, childPath); }
					catch (UnanimatableTargetException ex)
					{
						Trace.WriteLine($"Storyboard: animation of '{childPath?.Path}' skipped: {ex.Message}");
						continue;
					}
					// Clocks of one Begin layer on a property; only the first replaces earlier runs.
					var hand = bound.Add(resolved) ? handoff : HandoffBehavior.Compose;
					AnimationStore.Apply(resolved.Item1, resolved.Item2, clock, hand);
					leaves.Add(clock);
					break;
			}
		}
	}

	// WPF applies a MediaClock by handing it to the MediaElement named by TargetName/Target.
	static void BindMedia(MediaClock clock, FrameworkElement container, string? name, DependencyObject? target)
	{
		var obj = target;
		if (obj == null && !string.IsNullOrEmpty(name))
			obj = container.FindName(name!) as DependencyObject
				?? throw new InvalidOperationException($"Cannot find the element named '{name}' to play media.");
		obj ??= container;
		if (!(obj is Controls.MediaElement element))
			throw new InvalidOperationException("A MediaTimeline must target a MediaElement.");
		element.Clock = clock;
	}

	static (DependencyObject, DependencyProperty) ResolveTarget(
		FrameworkElement container, string? name, DependencyObject? target, PropertyPath? path)
	{
		if (path == null)
			throw new InvalidOperationException("Storyboard.TargetProperty is not set on an animation.");
		var obj = target;
		if (obj == null && !string.IsNullOrEmpty(name))
			obj = container.FindName(name!) as DependencyObject
				?? throw new InvalidOperationException(
					$"Cannot find the element named '{name}' to animate '{path.Path}'.");
		return StoryboardPathResolver.Resolve(obj ?? container, path);
	}
}
