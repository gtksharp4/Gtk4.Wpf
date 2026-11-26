using System.Collections.Generic;

namespace System.Windows.Media.Animation;

public partial class Storyboard
{
	/// <summary>
	/// The handoff for one clock of a Begin: clocks of one Begin layer on the same (object, property); only the
	/// first uses <paramref name="handoff"/> to replace earlier runs.
	/// </summary>
	static HandoffBehavior LayeredHandoff(
		HashSet<(DependencyObject, DependencyProperty)> bound, (DependencyObject, DependencyProperty) key,
		HandoffBehavior handoff) => bound.Add(key) ? handoff : HandoffBehavior.Compose;
}
