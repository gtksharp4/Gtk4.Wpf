using System.Collections.Generic;

namespace System.Windows.Automation;

/// <summary>What a <see cref="CacheRequest"/> captured for one element.</summary>
sealed class ElementCache
{
	internal Dictionary<int, object?> Properties { get; } = new Dictionary<int, object?>();

	internal Dictionary<int, BasePattern> Patterns { get; } = new Dictionary<int, BasePattern>();

	internal List<AutomationElement>? Children { get; set; }

	/// <summary>The cached requesting element, set on its cached children.</summary>
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
			{
				var childElement = new AutomationElement(child, null);
				var childCache = Take(childElement, related);
				childCache.ParentCached = true;
				cache.Children.Add(new AutomationElement(child, childCache));
			}
		}
		return cache;
	}
}

public partial class AutomationElement
{
	AutomationElement WithCache(CacheRequest request)
	{
		var cached = new AutomationElement(Peer, ElementCache.Take(this, request));
		if (cached._cache!.Children != null)
			foreach (var child in cached._cache.Children)
				child._cache!.Parent = cached;
		return cached;
	}

	/// <summary>The children cached with the element.</summary>
	/// <exception cref="InvalidOperationException">The children were not cached.</exception>
	public AutomationElementCollection CachedChildren =>
		_cache?.Children != null ? new AutomationElementCollection(_cache.Children)
			: throw new InvalidOperationException("The children were not cached.");

	/// <summary>The element whose request cached this one; only cached children have it.</summary>
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
