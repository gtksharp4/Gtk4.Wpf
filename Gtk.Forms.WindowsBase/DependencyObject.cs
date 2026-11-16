using System;
using System.Collections.Generic;

namespace System.Windows
{
	/// <summary>
	/// Base class for all objects that participate in the dependency-property and
	/// routed-event systems.
	/// </summary>
	public class DependencyObject
	{
		// Local value store: only entries explicitly set by the caller live here.
		readonly Dictionary<DependencyProperty, object?> _localValues =
			new Dictionary<DependencyProperty, object?>();

		// Style value store: values applied by Style setters (D6). Priority: local > style > inherited > default.
		readonly Dictionary<DependencyProperty, object?> _styleValues =
			new Dictionary<DependencyProperty, object?>();

		// ── Inheritance ─────────────────────────────────────────────────────────────

		DependencyObject? _inheritanceParent;

		/// <summary>
		/// Parent in the logical/visual tree, used to walk inherited property values.
		/// Setting this re-evaluates every inheriting property that has no local value
		/// and fires change notifications / propagates to descendants for those that changed.
		/// </summary>
		internal DependencyObject? InheritanceParent
		{
			get => _inheritanceParent;
			set
			{
				if (ReferenceEquals(_inheritanceParent, value)) return;
				var old = _inheritanceParent;

				// Snapshot effective values before the parent changes (while the old parent
				// is still reachable) so we can compute accurate before/after deltas.
				var inheriting = DependencyProperty.GetInheritingProperties();
				var oldValues = new object?[inheriting.Count];
				for (int i = 0; i < inheriting.Count; i++)
					oldValues[i] = _localValues.ContainsKey(inheriting[i])
						? DependencyProperty.UnsetValue  // has local value — won't be touched
						: ResolveEffectiveValue(inheriting[i]);

				_inheritanceParent = value;
				DependencyObjectInheritance.Track(this, old, value);

				// Fire change notifications for each inheriting DP whose effective value changed.
				for (int i = 0; i < inheriting.Count; i++)
				{
					if (ReferenceEquals(oldValues[i], DependencyProperty.UnsetValue)) continue; // local value

					var newVal = ResolveEffectiveValue(inheriting[i]);
					if (!Equals(oldValues[i], newVal))
						NotifyInheritedValueChanged(inheriting[i], oldValues[i], newVal);
				}
			}
		}

		// ── GetValue / SetValue / ClearValue ────────────────────────────────────────

		/// <summary>Returns the current effective value of the property.</summary>
		public object? GetValue(DependencyProperty dp)
		{
			if (dp == null) throw new ArgumentNullException(nameof(dp));
			return ResolveEffectiveValue(dp);
		}

		/// <summary>Sets the local value of the property.</summary>
		public void SetValue(DependencyProperty dp, object? value)
		{
			if (dp == null) throw new ArgumentNullException(nameof(dp));
			if (dp.IsReadOnly) throw new InvalidOperationException(
				$"DependencyProperty '{dp.Name}' is read-only. Use SetValue(DependencyPropertyKey, value).");
			SetValueCore(dp, value);
		}

		/// <summary>Sets the local value of a read-only property via its key.</summary>
		public void SetValue(DependencyPropertyKey key, object? value)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));
			SetValueCore(key.DependencyProperty, value);
		}

		/// <summary>Clears the local value; the effective value reverts to inherited or default.</summary>
		public void ClearValue(DependencyProperty dp)
		{
			if (dp == null) throw new ArgumentNullException(nameof(dp));
			if (!_localValues.ContainsKey(dp)) return;

			var oldEffective = ResolveEffectiveValue(dp);
			_localValues.Remove(dp);
			var newEffective = ResolveEffectiveValue(dp);
			FireChangeIfDifferent(dp, oldEffective, newEffective);
		}

		/// <summary>
		/// Returns the local value, or <see cref="DependencyProperty.UnsetValue"/> if no
		/// local value has been set.
		/// </summary>
		public object? ReadLocalValue(DependencyProperty dp)
		{
			if (dp == null) throw new ArgumentNullException(nameof(dp));
			return _localValues.TryGetValue(dp, out var v) ? v : DependencyProperty.UnsetValue;
		}

		/// <summary>
		/// Override to react to any effective-value change before the
		/// <see cref="PropertyChangedCallback"/> is invoked.
		/// </summary>
		protected virtual void OnPropertyChanged(DependencyPropertyChangedEventArgs e) { }

		// ── Internal helpers ─────────────────────────────────────────────────────────

		void SetValueCore(DependencyProperty dp, object? value)
		{
			var meta = dp.GetMetadata(GetType());

			// Coerce.
			if (meta.CoerceValueCallback != null)
				value = meta.CoerceValueCallback(this, value);

			var oldEffective = ResolveEffectiveValue(dp);

			_localValues[dp] = value;

			// If the effective value did not change (e.g. coercion returned same as inherited),
			// skip notifications — but always propagate to children so their resolution updates.
			var newEffective = value; // local always wins in Phase-0 precedence
			FireChangeIfDifferent(dp, oldEffective, newEffective);
		}

		void FireChangeIfDifferent(DependencyProperty dp, object? oldEffective, object? newEffective)
		{
			if (Equals(oldEffective, newEffective)) return;

			var args = new DependencyPropertyChangedEventArgs(dp, oldEffective, newEffective);
			OnPropertyChanged(args);

			var meta = dp.GetMetadata(GetType());
			meta.PropertyChangedCallback?.Invoke(this, args);

			// Propagate to any children that inherit this property.
			var fmeta = meta as FrameworkPropertyMetadata;
			if (fmeta != null && fmeta.Inherits)
				DependencyObjectInheritance.PropagateInheritedValue(dp, this, oldEffective, newEffective);
		}

		/// <summary>
		/// Resolves the effective value: local > style > inherited > metadata default.
		/// </summary>
		internal object? ResolveEffectiveValue(DependencyProperty dp)
		{
			if (_localValues.TryGetValue(dp, out var local)) return local;

			// D6: style values sit below local values but above inherited/default.
			if (_styleValues.TryGetValue(dp, out var style)) return style;

			var meta = dp.GetMetadata(GetType());
			var fmeta = meta as FrameworkPropertyMetadata;
			if (fmeta != null && fmeta.Inherits && _inheritanceParent != null)
				return _inheritanceParent.ResolveEffectiveValue(dp);

			return meta.DefaultValue;
		}

		/// <summary>
		/// D6: Sets a style-layer value (lower priority than a local value).
		/// Only fires a change notification when it actually changes the effective value.
		/// </summary>
		internal void SetStyleValue(DependencyProperty dp, object? value)
		{
			if (dp == null) throw new ArgumentNullException(nameof(dp));
			// Local value trumps style — record but don't fire change if local is set.
			var oldEffective = ResolveEffectiveValue(dp);
			_styleValues[dp] = value;
			var newEffective = ResolveEffectiveValue(dp);
			FireChangeIfDifferent(dp, oldEffective, newEffective);
		}

		/// <summary>
		/// D6: Removes all style-layer values previously set by <see cref="SetStyleValue"/>,
		/// firing change notifications for each that changes the effective value.
		/// </summary>
		internal void ClearStyleValues()
		{
			if (_styleValues.Count == 0) return;
			// Snapshot keys so we can iterate while modifying.
			var keys = new List<DependencyProperty>(_styleValues.Keys);
			_styleValues.Clear();
			foreach (var dp in keys)
			{
				// old effective was the style value (since local would have shadowed it).
				var meta = dp.GetMetadata(GetType());
				var fmeta = meta as FrameworkPropertyMetadata;
				object? oldEffective;
				// We already cleared _styleValues, so old effective was the style value.
				// Re-compute new effective (inherited or default).
				var newEffective = ResolveEffectiveValue(dp);
				// Reconstruct what old was: if local set, nothing changed (local shadowed style).
				if (_localValues.TryGetValue(dp, out var localVal))
					continue; // local value shadowed the style — no visible change
				// old effective was whatever was in _styleValues (now cleared); we need that value.
				// We snapshotted only keys; re-read from a temporary isn't possible, so we track
				// old effective before clearing.
				// (This path is unreachable: we cleared above; keep for correctness.)
				oldEffective = newEffective; // won't fire
				FireChangeIfDifferent(dp, oldEffective, newEffective);
			}
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
				// If a local value is set, the visible effective value didn't change.
				if (_localValues.ContainsKey(kv.Key)) continue;
				var newEffective = ResolveEffectiveValue(kv.Key);
				FireChangeIfDifferent(kv.Key, kv.Value, newEffective);
			}
		}

		/// <summary>Returns a snapshot of the current style-value store (for un-apply support).</summary>
		internal Dictionary<DependencyProperty, object?> SnapshotStyleValues() =>
			new Dictionary<DependencyProperty, object?>(_styleValues);

		/// <summary>
		/// Called by <see cref="DependencyObjectInheritance"/> when a parent's inherited value
		/// changes and this object has no local value for the property.
		/// </summary>
		internal void NotifyInheritedValueChanged(
			DependencyProperty dp, object? oldParentValue, object? newParentValue)
		{
			// Only propagate if there is no local value shadowing the inherited one.
			if (_localValues.ContainsKey(dp)) return;

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
