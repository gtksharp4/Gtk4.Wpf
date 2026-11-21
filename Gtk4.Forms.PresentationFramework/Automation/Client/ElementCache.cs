using System.Collections.Generic;

namespace System.Windows.Automation;

/// <summary>What a <see cref="CacheRequest"/> captured for one element.</summary>
sealed class ElementCache
{
	internal Dictionary<int, object?> Properties { get; } = new Dictionary<int, object?>();

	internal Dictionary<int, BasePattern> Patterns { get; } = new Dictionary<int, BasePattern>();

	internal List<AutomationElement>? Children { get; set; }

	internal AutomationElement? Parent { get; set; }

	internal bool ParentCached { get; set; }

	/// <summary>Reads the requested properties and patterns; runs on the GTK thread.</summary>
	internal static ElementCache Take(AutomationElement element, CacheRequest request)
	{
		var cache = new ElementCache();
		var related = request.Clone();
		related.TreeScope = TreeScope.Element;
		foreach (var property in request.Properties)
			cache.Properties[property.Id] = element.ReadPropertyCore(property, ignoreDefault: true);
		foreach (var pattern in request.Patterns)
			if (PatternRegistry.Create(pattern, element, cached: true) is { } captured)
				cache.Patterns[pattern.Id] = captured;
		if ((request.TreeScope & TreeScope.Children) != 0)
		{
			cache.Children = new List<AutomationElement>();
			foreach (var child in ElementNavigation.Children(element.Peer, request.TreeFilter))
				cache.Children.Add(AutomationElement.Wrap(child, related));
		}
		if ((request.TreeScope & TreeScope.Parent) != 0)
		{
			cache.ParentCached = true;
			if (ElementNavigation.Parent(element.Peer, request.TreeFilter) is { } parent)
				cache.Parent = AutomationElement.Wrap(parent, related);
		}
		return cache;
	}
}

public partial class AutomationElement
{
	AutomationElement WithCache(CacheRequest request) => new AutomationElement(Peer, ElementCache.Take(this, request));

	/// <summary>The children cached with the element.</summary>
	/// <exception cref="InvalidOperationException">The children were not cached.</exception>
	public AutomationElementCollection CachedChildren =>
		_cache?.Children != null ? new AutomationElementCollection(_cache.Children)
			: throw new InvalidOperationException("The children were not cached.");

	/// <summary>The parent cached with the element, or <c>null</c> when it has none.</summary>
	/// <exception cref="InvalidOperationException">The parent was not cached.</exception>
	public AutomationElement CachedParent =>
		_cache != null && _cache.ParentCached
			? _cache.Parent!
			: throw new InvalidOperationException("The parent was not cached.");

	/// <summary>Returns a copy of the element holding a fresh cache for <paramref name="request"/>.</summary>
	public AutomationElement GetUpdatedCache(CacheRequest request)
	{
		if (request == null) throw new ArgumentNullException(nameof(request));
		var snapshot = request.Clone();
		return AutomationThread.Call(() =>
		{
			EnsureAvailable();
			return WithCache(snapshot);
		});
	}
}
