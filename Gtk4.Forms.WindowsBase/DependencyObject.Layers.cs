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
	/// Stores the raw value; coercion is applied on top (see <c>RecomputeCoerced</c>).
	/// </summary>
	internal void SetStyleValue(DependencyProperty dp, object? value)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		var oldEffective = ResolveEffectiveValue(dp);
		_styleValues[dp] = value;
		CommitLayerChange(dp, oldEffective);
	}

	/// <summary>Removes one style-layer value, raising the change when the effective value moves.</summary>
	internal void ClearStyleValue(DependencyProperty dp)
	{
		if (dp == null || !_styleValues.ContainsKey(dp)) return;
		var oldEffective = ResolveEffectiveValue(dp);
		_styleValues.Remove(dp);
		CommitLayerChange(dp, oldEffective);
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
			var oldEffective = ResolveEffectiveValue(kv.Key);
			_styleValues.Remove(kv.Key);
			CommitLayerChange(kv.Key, oldEffective);
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
	/// Stores the raw value; coercion is applied on top.
	/// </summary>
	internal void SetTriggerValue(DependencyProperty dp, object contributor, object? value)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		if (contributor == null) throw new ArgumentNullException(nameof(contributor));

		var oldEffective = ResolveEffectiveValue(dp);

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

		CommitLayerChange(dp, oldEffective);
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

		CommitLayerChange(dp, oldEffective);
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
		// Any layer above Inherited shadows the inherited value.
		if (_localValues.ContainsKey(dp) || !ReferenceEquals(GetTopTriggerValue(dp), DependencyProperty.UnsetValue)
		    || _animationValues.ContainsKey(dp) || _dynResValues.ContainsKey(dp) || _styleValues.ContainsKey(dp))
		{
			// Shadowed: the value is unchanged, but the inherited fallback it is compared with moved.
			NotifyValueSource(dp);
			return;
		}

		var meta = dp.GetMetadata(GetType());
		if (meta.CoerceValueCallback != null)
		{
			// A coerced result may depend on the inherited value: compare effective values.
			if (_coercedValues != null && _coercedValues.TryGetValue(dp, out var oldCoerced)) oldParentValue = oldCoerced;
			RecomputeCoerced(dp);
			newParentValue = ResolveEffectiveValue(dp);
			if (Equals(oldParentValue, newParentValue)) return;
		}

		var args = new DependencyPropertyChangedEventArgs(dp, oldParentValue, newParentValue);
		OnPropertyChanged(args);
		meta.PropertyChangedCallback?.Invoke(this, args);

		var fmeta = meta as FrameworkPropertyMetadata;
		if (fmeta != null && fmeta.Inherits)
			DependencyObjectInheritance.PropagateInheritedValue(dp, this, oldParentValue, newParentValue);
	}
}