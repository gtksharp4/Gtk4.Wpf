namespace System.Windows.Data;

/// <summary>
/// Target-kind adapter for the binding engine. Elements (<see cref="FrameworkElement"/>,
/// <see cref="FrameworkContentElement"/>) resolve directly; any other <see cref="DependencyObject"/> (a freezable,
/// an animation, a <c>CollectionViewSource</c>) resolves through its mentor: the element reached along the
/// inheritance context. Content elements reach the hosting element along their logical parents.
/// </summary>
internal static class BindingTarget
{
	internal static bool IsElement(DependencyObject target) =>
		target is FrameworkElement || target is FrameworkContentElement;

	/// <summary>The nearest element at or above <paramref name="target"/> along the context chain.</summary>
	internal static DependencyObject? MentorOf(DependencyObject target)
	{
		DependencyObject? current = target;
		for (int steps = 0; current != null && steps < 1000; steps++)
		{
			if (IsElement(current)) return current;
			current = current.ContextParent;
		}
		return null;
	}

	internal static object? DataContextOf(DependencyObject target) => target switch
	{
		FrameworkElement fe => fe.DataContext,
		FrameworkContentElement fce => fce.DataContext,
		_ => MentorOf(target) is { } mentor ? DataContextOf(mentor) : null
	};

	/// <summary>The nearest <see cref="FrameworkElement"/> at or above <paramref name="target"/>.</summary>
	internal static FrameworkElement? HostOf(DependencyObject target)
	{
		DependencyObject? current = target;
		for (int steps = 0; current != null && steps < 1000; steps++)
		{
			if (current is FrameworkElement fe) return fe;
			current = current is FrameworkContentElement fce ? fce.Parent : current.ContextParent;
		}
		return null;
	}

	internal static object? FindName(DependencyObject target, string name) => HostOf(target)?.FindName(name);

	internal static object? ResolveRelative(DependencyObject target, RelativeSource source)
	{
		if (target is FrameworkElement fe) return source.Resolve(fe);
		if (source.Mode == RelativeSourceMode.Self) return target;
		if (!IsElement(target)) return ResolveFromMentor(target, source);
		if (source.Mode == RelativeSourceMode.FindAncestor) return source.FindAncestor(target);
		var host = HostOf(target);
		return host == null ? null : source.Resolve(host);
	}

	static object? ResolveFromMentor(DependencyObject target, RelativeSource source)
	{
		var mentor = MentorOf(target);
		if (mentor == null) return null;
		if (source.Mode != RelativeSourceMode.FindAncestor) return ResolveRelative(mentor, source);
		if (source.AncestorType?.IsInstanceOfType(mentor) != true) return source.FindAncestor(mentor);
		return source.AncestorLevel <= 1
			? mentor
			: new RelativeSource(RelativeSourceMode.FindAncestor, source.AncestorType, source.AncestorLevel - 1)
				.FindAncestor(mentor);
	}
}
