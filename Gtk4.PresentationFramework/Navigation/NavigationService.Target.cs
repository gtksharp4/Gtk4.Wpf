using System.Collections.Generic;
using System.Windows.Controls;

namespace System.Windows.Navigation;

public sealed partial class NavigationService
{
	/// <summary>The nearest navigator (Frame or NavigationWindow service) above <paramref name="start"/>.</summary>
	internal static NavigationService? FindNavigator(DependencyObject? start)
	{
		for (var current = start; current != null; current = ParentOf(current))
		{
			if (current is Frame frame) return frame.NavigationService;
			if (current is NavigationWindow window) return window.NavigationService;
		}
		return null;
	}

	/// <summary>
	/// The navigator named <paramref name="name"/> (WPF FindTarget): frames inside the nearest navigator's
	/// content first, then frames in each ancestor navigator.
	/// </summary>
	internal static NavigationService? FindTarget(DependencyObject? start, string name)
	{
		var seen = new HashSet<DependencyObject>();
		for (var current = start; current != null; current = ParentOf(current))
		{
			if (FindNamed(current, name, seen) is { } found) return found;
		}
		return null;
	}

	static NavigationService? FindNamed(DependencyObject root, string name, HashSet<DependencyObject> seen)
	{
		if (!seen.Add(root)) return null;
		if (root is Frame frame && frame.Name == name) return frame.NavigationService;
		if (root is NavigationWindow window && window.Name == name) return window.NavigationService;
		if (root is UIElement element)
			foreach (var child in element.VisualChildren)
				if (FindNamed(child, name, seen) is { } found) return found;
		if (root is NavigationWindow nw && nw.NavigationService.Content is UIElement content)
			return FindNamed(content, name, seen);
		return null;
	}

	static DependencyObject? ParentOf(DependencyObject d) => (d as UIElement)?.LogicalParent ?? d.InheritanceParent;

	/// <summary>
	/// Resolves <paramref name="uri"/> against the directory of this navigator's current source;
	/// backslashes count as slashes, as in WPF's pack URI handling.
	/// </summary>
	internal Uri ResolveRelative(Uri uri)
	{
		if (uri.IsAbsoluteUri) return uri;
		var path = uri.OriginalString.Replace('\\', '/');
		var baseText = CurrentSource?.OriginalString.Replace('\\', '/');
		var cut = baseText?.LastIndexOf('/') ?? -1;
		if (baseText != null && cut >= 0 && !path.StartsWith("/", StringComparison.Ordinal))
			path = baseText.Substring(0, cut + 1) + path;
		return new Uri(path, UriKind.RelativeOrAbsolute);
	}
}
