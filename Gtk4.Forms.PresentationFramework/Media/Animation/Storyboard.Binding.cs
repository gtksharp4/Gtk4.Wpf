using System.Collections.Generic;

namespace System.Windows.Media.Animation;

public partial class Storyboard
{
	/// <summary>Applies every animation clock below <paramref name="group"/> to its target property.</summary>
	void BindClocks(
		ClockGroup group, FrameworkElement container, string? name, DependencyObject? target,
		PropertyPath? path, HandoffBehavior handoff, List<AnimationClock> leaves)
	{
		for (var i = 0; i < group.Children.Count; i++)
		{
			var timeline = group.Timeline.Children[i];
			var childName = (string?)timeline.GetValue(TargetNameProperty) ?? name;
			var childTarget = (DependencyObject?)timeline.GetValue(TargetProperty) ?? target;
			var childPath = (PropertyPath?)timeline.GetValue(TargetPropertyProperty) ?? path;
			switch (group.Children[i])
			{
				case ClockGroup nested:
					BindClocks(nested, container, childName, childTarget, childPath, handoff, leaves);
					break;
				case AnimationClock clock:
					var (obj, dp) = ResolveTarget(container, childName, childTarget, childPath);
					AnimationStore.Apply(obj, dp, clock, handoff);
					leaves.Add(clock);
					break;
			}
		}
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
