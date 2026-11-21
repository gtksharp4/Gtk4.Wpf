namespace System.Windows.Data;

/// <summary>
/// Target-kind adapter for the binding engine: a <see cref="FrameworkElement"/> or a
/// <see cref="FrameworkContentElement"/>. Content elements resolve names and relative sources through
/// the hosting element reached along their logical parents.
/// </summary>
internal static class BindingTarget
{
	internal static object? DataContextOf(DependencyObject target) => target switch
	{
		FrameworkElement fe => fe.DataContext,
		FrameworkContentElement fce => fce.DataContext,
		_ => null
	};

	/// <summary>The nearest <see cref="FrameworkElement"/> at or above <paramref name="target"/>.</summary>
	internal static FrameworkElement? HostOf(DependencyObject target)
	{
		DependencyObject? current = target;
		for (int steps = 0; current != null && steps < 1000; steps++)
		{
			if (current is FrameworkElement fe) return fe;
			current = current is FrameworkContentElement fce ? fce.Parent : null;
		}
		return null;
	}

	internal static object? FindName(DependencyObject target, string name) => HostOf(target)?.FindName(name);

	internal static object? ResolveRelative(DependencyObject target, RelativeSource source)
	{
		if (target is FrameworkElement fe) return source.Resolve(fe);
		if (source.Mode == RelativeSourceMode.Self) return target;
		var host = HostOf(target);
		return host == null ? null : source.Resolve(host);
	}
}
