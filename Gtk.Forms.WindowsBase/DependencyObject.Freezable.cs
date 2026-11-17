using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Freezable-support helpers for <see cref="DependencyObject"/>.
/// </summary>
public partial class DependencyObject
{
	/// <summary>
	/// Returns a snapshot of all locally-set dependency property values.
	/// Used by <see cref="Freezable.Clone"/> to copy state to the new instance.
	/// </summary>
	internal IEnumerable<KeyValuePair<DependencyProperty, object?>> GetLocalValueSnapshot()
	{
		// Snapshot the dict to avoid mutating during enumeration in clone callbacks.
		var snapshot = new List<KeyValuePair<DependencyProperty, object?>>(_localValues.Count);
		foreach (var kv in _localValues)
			snapshot.Add(new KeyValuePair<DependencyProperty, object?>(kv.Key, kv.Value));
		return snapshot;
	}
}
