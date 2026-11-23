namespace System.Windows.Media.Animation;

public partial class Storyboard
{
	/// <summary>
	/// Makes <paramref name="container"/> the inheritance context of the storyboard and, through each group, of
	/// every child timeline, so bindings on animation properties resolve against the element that starts it.
	/// </summary>
	void AdoptContext(FrameworkElement container)
	{
		InheritanceContext = container;
		AdoptChildren(this);
	}

	static void AdoptChildren(TimelineGroup group)
	{
		foreach (var child in group.Children)
		{
			child.InheritanceContext = group;
			if (child is TimelineGroup nested) AdoptChildren(nested);
		}
	}
}
