using System;
using System.Collections.Generic;

namespace System.Windows
{
	/// <summary>
	/// Trigger-value precedence layer for <see cref="DependencyObject"/>.
	/// Priority: local > trigger > style > inherited > default.
	/// </summary>
	public partial class DependencyObject
	{
		// Trigger value store: values applied by Trigger/DataTrigger/MultiTrigger setters (D8).
		readonly Dictionary<DependencyProperty, object?> _triggerValues =
			new Dictionary<DependencyProperty, object?>();

		// Per-DP change hooks used by triggers to subscribe to property changes.
		Dictionary<DependencyProperty, List<PropertyChangedCallback>>? _changeHooks;

		// ── Style value layer (D6) ──────────────────────────────────────────────────

		/// <summary>
		/// D6: Sets a style-layer value (lower priority than a local value).
		/// Only fires a change notification when it actually changes the effective value.
		/// </summary>
		internal void SetStyleValue(DependencyProperty dp, object? value)
		{
			if (dp == null) throw new ArgumentNullException(nameof(dp));
			var oldEffective = ResolveEffectiveValue(dp);
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
		/// D8: Sets a trigger-layer value (above style, below local).
		/// Only fires a change notification when the effective value actually changes.
		/// </summary>
		internal void SetTriggerValue(DependencyProperty dp, object? value)
		{
			if (dp == null) throw new ArgumentNullException(nameof(dp));
			var oldEffective = ResolveEffectiveValue(dp);
			_triggerValues[dp] = value;
			var newEffective = ResolveEffectiveValue(dp);
			FireChangeIfDifferent(dp, oldEffective, newEffective);
		}

		/// <summary>
		/// D8: Removes trigger-layer values given a snapshot, restoring to style/inherited/default.
		/// </summary>
		internal void ClearTriggerValues(Dictionary<DependencyProperty, object?> snapshot)
		{
			if (snapshot == null || snapshot.Count == 0) return;
			foreach (var kv in snapshot)
			{
				_triggerValues.Remove(kv.Key);
				if (_localValues.ContainsKey(kv.Key)) continue;
				var newEffective = ResolveEffectiveValue(kv.Key);
				FireChangeIfDifferent(kv.Key, kv.Value, newEffective);
			}
		}

		/// <summary>Returns a snapshot of current trigger-value store (for un-apply).</summary>
		internal Dictionary<DependencyProperty, object?> SnapshotTriggerValues() =>
			new Dictionary<DependencyProperty, object?>(_triggerValues);

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
			if (_triggerValues.ContainsKey(dp)) return;

			var args = new DependencyPropertyChangedEventArgs(dp, oldParentValue, newParentValue);
			OnPropertyChanged(args);

			var meta = dp.GetMetadata(GetType());
			meta.PropertyChangedCallback?.Invoke(this, args);

			var fmeta = meta as FrameworkPropertyMetadata;
			if (fmeta != null && fmeta.Inherits)
				DependencyObjectInheritance.PropagateInheritedValue(dp, this, oldParentValue, newParentValue);
		}
	}
}
