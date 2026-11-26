using System.Collections.Generic;

namespace System.Windows.Automation;

public partial class AutomationElement
{
	/// <summary>Returns the pattern object for <paramref name="pattern"/>.</summary>
	/// <exception cref="InvalidOperationException">The element does not support the pattern.</exception>
	public object GetCurrentPattern(AutomationPattern pattern) =>
		TryGetCurrentPattern(pattern, out var result) ? result!
			: throw new InvalidOperationException("Unsupported Pattern.");

	public bool TryGetCurrentPattern(AutomationPattern pattern, out object? patternObject)
	{
		if (pattern == null) throw new ArgumentNullException(nameof(pattern));
		patternObject = AutomationThread.Call(() =>
		{
			EnsureAvailable();
			return PatternRegistry.Create(pattern, this, cached: false);
		});
		return patternObject != null;
	}

	/// <summary>Returns the pattern object cached with the element.</summary>
	/// <exception cref="InvalidOperationException">The pattern was not cached.</exception>
	public object GetCachedPattern(AutomationPattern pattern) =>
		TryGetCachedPattern(pattern, out var result) ? result!
			: throw new InvalidOperationException("The pattern was not cached.");

	public bool TryGetCachedPattern(AutomationPattern pattern, out object? patternObject)
	{
		if (pattern == null) throw new ArgumentNullException(nameof(pattern));
		BasePattern? found = null;
		var known = _cache?.Patterns.TryGetValue(pattern.Id, out found) == true;
		patternObject = found;
		return known;
	}

	/// <summary>Returns the patterns the element supports.</summary>
	public AutomationPattern[] GetSupportedPatterns() => AutomationThread.Call(() =>
	{
		EnsureAvailable();
		var result = new List<AutomationPattern>();
		foreach (var pattern in PatternRegistry.All)
			if (PatternRegistry.Create(pattern, this, cached: false) != null)
				result.Add(pattern);
		return result.ToArray();
	});
}
