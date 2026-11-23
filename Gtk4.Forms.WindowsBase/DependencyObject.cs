using System;
using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Base class for all objects that participate in the dependency-property and
/// routed-event systems.
/// </summary>
public partial class DependencyObject : System.Windows.Threading.DispatcherObject
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
			InheritanceParentChangedHook?.Invoke(this);
		}
	}

	// ── GetValue / SetValue / ClearValue ────────────────────────────────────────

	/// <summary>Returns the current effective value of the property.</summary>
	public object? GetValue(DependencyProperty dp)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		return ResolveEffectiveValue(dp);
	}

	/// <summary>Sets the local value of the property; <see cref="DependencyProperty.UnsetValue"/> clears it.</summary>
	public void SetValue(DependencyProperty dp, object? value)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		if (dp.IsReadOnly) throw new InvalidOperationException(
			$"DependencyProperty '{dp.Name}' is read-only. Use SetValue(DependencyPropertyKey, value).");
		if (ReferenceEquals(value, DependencyProperty.UnsetValue)) { ClearValue(dp); return; }
		SetValueCore(dp, value);
	}

	/// <summary>Sets the local value of a read-only property via its key.</summary>
	public void SetValue(DependencyPropertyKey key, object? value)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		if (ReferenceEquals(value, DependencyProperty.UnsetValue)) { ClearValue(key.DependencyProperty); return; }
		SetValueCore(key.DependencyProperty, value);
	}

	/// <summary>Clears the local value; the effective value reverts to inherited or default.</summary>
	public void ClearValue(DependencyProperty dp)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		OnExplicitLocalChange(dp);
		if (!_localValues.ContainsKey(dp)) return;

		var oldEffective = ResolveEffectiveValue(dp);
		_localValues.Remove(dp);
		CommitLayerChange(dp, oldEffective);
	}

	/// <summary>
	/// Returns the raw (un-coerced) local value, or <see cref="DependencyProperty.UnsetValue"/> if no
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
	/// Fires per-DP change hooks registered by triggers.
	/// </summary>
	protected virtual void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (_changeHooks != null && _changeHooks.TryGetValue(e.Property, out var hooks))
		{
			// Snapshot so a hook may un-/re-subscribe without invalidating iteration.
			var snap = new List<PropertyChangedCallback>(hooks);
			foreach (var hook in snap)
				hook(this, e);
		}
	}

	// ── Internal helpers ─────────────────────────────────────────────────────────

	/// <summary>
	/// Called by <see cref="SetValueCore"/> before writing a new local value.
	/// Override (e.g. in <see cref="Freezable"/>) to enforce mutation guards.
	/// </summary>
	protected virtual void OnBeforeSetValue(DependencyProperty dp) { }

	void SetValueCore(DependencyProperty dp, object? value)
	{
		if (!ReferenceEquals(value, DependencyProperty.UnsetValue)) dp.ThrowIfInvalid(value);
		OnBeforeSetValue(dp);
		OnExplicitLocalChange(dp);
		var oldEffective = ResolveEffectiveValue(dp);
		bool hadLocal = _localValues.TryGetValue(dp, out var oldLocal);
		_localValues[dp] = value;
		// A throwing coerce callback stores nothing (WPF).
		try { RecomputeCoerced(dp); }
		catch
		{
			if (hadLocal) _localValues[dp] = oldLocal;
			else _localValues.Remove(dp);
			RecomputeCoerced(dp);
			throw;
		}
		FireChangeIfDifferent(dp, oldEffective, ResolveEffectiveValue(dp));
		NotifyValueSource(dp);
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
	/// Resolves the effective value:
	/// animation &gt; local &gt; trigger &gt; dynamic-resource &gt; style &gt; inherited &gt; metadata default.
	/// </summary>
	internal object? ResolveEffectiveValue(DependencyProperty dp)
	{
		// Coercion result for a property with no local value (DependencyObject.Coerce.cs).
		if (_coercedValues != null && _coercedValues.TryGetValue(dp, out var coerced)) return coerced;

		// D10: animation values sit above everything else (WPF precedence).
		if (_animationValues.TryGetValue(dp, out var anim)) return anim;

		if (_localValues.TryGetValue(dp, out var local)) return local;

		// D8: trigger values sit above dynamic-resource and style but below local.
		var triggerTop = GetTopTriggerValue(dp);
		if (!ReferenceEquals(triggerTop, DependencyProperty.UnsetValue)) return triggerTop;

		// D7: dynamic-resource layer — below trigger, above style.
		if (_dynResValues.TryGetValue(dp, out var dynRes)) return dynRes;

		// D6: style values sit below dynamic-resource but above inherited/default.
		if (_styleValues.TryGetValue(dp, out var style)) return style;

		var meta = dp.GetMetadata(GetType());
		var fmeta = meta as FrameworkPropertyMetadata;
		if (fmeta != null && fmeta.Inherits && _inheritanceParent != null)
			return _inheritanceParent.ResolveEffectiveValue(dp);

		return meta.DefaultValue;
	}

}