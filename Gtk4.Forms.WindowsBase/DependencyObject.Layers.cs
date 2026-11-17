using System;
using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Trigger-value precedence layer for <see cref="DependencyObject"/>.
/// Priority: local &gt; trigger &gt; dynamic-resource &gt; style &gt; inherited &gt; default.
/// (The dynamic-resource layer lives in <c>DependencyObject.DynamicResourceLayer.cs</c>.)
/// </summary>
public partial class DependencyObject
{
	// Trigger value store: per-DP ordered list of (contributor, value) pairs.
	// Last entry is the highest-priority (last-declared) trigger for that DP.
	// E1: keyed by contributing trigger instance so siblings do not stomp each other.
	readonly Dictionary<DependencyProperty, List<(object contributor, object? value)>> _triggerValues =
		new Dictionary<DependencyProperty, List<(object, object?)>>();

	// Per-DP change hooks used by triggers to subscribe to property changes.
	Dictionary<DependencyProperty, List<PropertyChangedCallback>>? _changeHooks;

	// ── Style value layer (D6) ──────────────────────────────────────────────────

	/// <summary>
	/// D6: Sets a style-layer value (lower priority than a local value).
	/// Runs <see cref="PropertyMetadata.CoerceValueCallback"/> on the resulting effective
	/// value before firing change notifications (E4).
	/// </summary>
	internal void SetStyleValue(DependencyProperty dp, object? value)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		var oldEffective = ResolveEffectiveValue(dp);

		// Coerce the raw setter value before storing.
		var meta = dp.GetMetadata(GetType());
		if (meta.CoerceValueCallback != null)
			value = meta.CoerceValueCallback(this, value);

		_styleValues[dp] = value;
		var newEffective = ResolveEffectiveValue(dp);
		FireChangeIfDifferent(dp, oldEffective, newEffective);
	}

	/// <summary>
	/// D6: Removes style-layer values and fires change notifications correctly.
	/// Caller passes the snapshot of old style values before clearing.
	/// </summary>
	internal void ClearStyleValues(Dictionary<DependencyProperty, object?> snapshot)
	{
		if (snapshot == null || snapshot.Count == 0) return;
		foreach (var kv in snapshot)
		{
			_styleValues.Remove(kv.Key);
			if (_localValues.ContainsKey(kv.Key)) continue;
			var newEffective = ResolveEffectiveValue(kv.Key);
			FireChangeIfDifferent(kv.Key, kv.Value, newEffective);
		}
	}

	/// <summary>Returns a snapshot of the current style-value store (for un-apply support).</summary>
	internal Dictionary<DependencyProperty, object?> SnapshotStyleValues() =>
		new Dictionary<DependencyProperty, object?>(_styleValues);

	// ── Trigger value layer (D8) ─────────────────────────────────────────────

	/// <summary>
	/// D8: Pushes or updates a trigger-layer value from a specific <paramref name="contributor"/>
	/// (a <see cref="Trigger"/> / <see cref="DataTrigger"/> / <see cref="MultiTrigger"/> state).
	/// Last-declared contributor wins (last in insertion order).
	/// Runs <see cref="PropertyMetadata.CoerceValueCallback"/> on the value before storing (E4).
	/// </summary>
	internal void SetTriggerValue(DependencyProperty dp, object contributor, object? value)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		if (contributor == null) throw new ArgumentNullException(nameof(contributor));

		var oldEffective = ResolveEffectiveValue(dp);

		// Coerce the raw setter value before storing.
		var meta = dp.GetMetadata(GetType());
		if (meta.CoerceValueCallback != null)
			value = meta.CoerceValueCallback(this, value);

		if (!_triggerValues.TryGetValue(dp, out var list))
			_triggerValues[dp] = list = new List<(object, object?)>();

		// Update existing entry for this contributor, or append a new one.
		bool found = false;
		for (int i = 0; i < list.Count; i++)
		{
			if (ReferenceEquals(list[i].contributor, contributor))
			{
				list[i] = (contributor, value);
				found = true;
				break;
			}
		}
		if (!found) list.Add((contributor, value));

		var newEffective = ResolveEffectiveValue(dp);
		FireChangeIfDifferent(dp, oldEffective, newEffective);
	}

	/// <summary>
	/// D8: Removes the <paramref name="contributor"/>'s entry for <paramref name="dp"/>.
	/// Resolves to the previous contributor's value (or clears the layer if none remain).
	/// </summary>
	internal void ClearTriggerValue(DependencyProperty dp, object contributor)
	{
		if (dp == null || contributor == null) return;
		if (!_triggerValues.TryGetValue(dp, out var list)) return;

		var oldEffective = ResolveEffectiveValue(dp);

		for (int i = list.Count - 1; i >= 0; i--)
		{
			if (ReferenceEquals(list[i].contributor, contributor))
			{
				list.RemoveAt(i);
				break;
			}
		}
		if (list.Count == 0) _triggerValues.Remove(dp);

		if (_localValues.ContainsKey(dp)) return;
		var newEffective = ResolveEffectiveValue(dp);
		FireChangeIfDifferent(dp, oldEffective, newEffective);
	}

	/// <summary>
	/// D8 (batch): Removes all entries for the DPs in <paramref name="dps"/> that belong
	/// to <paramref name="contributor"/>. Used by trigger un-apply.
	/// </summary>
	internal void ClearTriggerValues(IEnumerable<DependencyProperty> dps, object contributor)
	{
		if (dps == null || contributor == null) return;
		foreach (var dp in dps)
			ClearTriggerValue(dp, contributor);
	}

	/// <summary>
	/// Returns the current top-of-stack trigger value for <paramref name="dp"/>,
	/// or <see cref="DependencyProperty.UnsetValue"/> when no trigger is active.
	/// </summary>
	internal object? GetTopTriggerValue(DependencyProperty dp)
	{
		if (!_triggerValues.TryGetValue(dp, out var list) || list.Count == 0)
			return DependencyProperty.UnsetValue;
		return list[list.Count - 1].value;
	}

	/// <summary>
	/// Subscribes <paramref name="callback"/> to effective-value changes of <paramref name="dp"/>.
	/// Used by triggers to watch property changes without polluting metadata.
	/// </summary>
	internal void AddChangeHook(DependencyProperty dp, PropertyChangedCallback callback)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		if (callback == null) throw new ArgumentNullException(nameof(callback));
		_changeHooks ??= new Dictionary<DependencyProperty, List<PropertyChangedCallback>>();
		if (!_changeHooks.TryGetValue(dp, out var list))
			_changeHooks[dp] = list = new List<PropertyChangedCallback>();
		list.Add(callback);
	}

	/// <summary>Removes a previously added change hook.</summary>
	internal void RemoveChangeHook(DependencyProperty dp, PropertyChangedCallback callback)
	{
		if (dp == null || callback == null) return;
		if (_changeHooks == null) return;
		if (!_changeHooks.TryGetValue(dp, out var list)) return;
		list.Remove(callback);
	}

	// ── Inheritance propagation ──────────────────────────────────────────────

	/// <summary>
	/// Called by <see cref="DependencyObjectInheritance"/> when a parent's inherited value
	/// changes and this object has no local value for the property.
	/// </summary>
	internal void NotifyInheritedValueChanged(
		DependencyProperty dp, object? oldParentValue, object? newParentValue)
	{
		// Propagate only when neither local nor trigger value shadows the inherited one.
		if (_localValues.ContainsKey(dp)) return;
		if (_triggerValues.ContainsKey(dp)) return; // any active contributor shadows inherited

		var args = new DependencyPropertyChangedEventArgs(dp, oldParentValue, newParentValue);
		OnPropertyChanged(args);

		var meta = dp.GetMetadata(GetType());
		meta.PropertyChangedCallback?.Invoke(this, args);

		var fmeta = meta as FrameworkPropertyMetadata;
		if (fmeta != null && fmeta.Inherits)
			DependencyObjectInheritance.PropagateInheritedValue(dp, this, oldParentValue, newParentValue);
	}
}