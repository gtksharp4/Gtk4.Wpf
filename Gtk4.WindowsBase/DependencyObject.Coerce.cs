using System;
using System.Collections.Generic;

namespace System.Windows;

public partial class DependencyObject
{
	// Coerced result for a property whose coercion changed the resolved value (e.g. IsEnabled
	// forced false by a parent). Sits above every other layer, as coercion does in WPF.
	// All layers store raw values, so coercion is reversible whatever the source.
	Dictionary<DependencyProperty, object?>? _coercedValues;

	/// <summary>
	/// Re-runs the property's <see cref="PropertyMetadata.CoerceValueCallback"/> against the current
	/// base value. The change callback and <c>OnPropertyChanged</c> run only if the effective value changes.
	/// </summary>
	public void CoerceValue(DependencyProperty dp)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		if (dp.GetMetadata(GetType()).CoerceValueCallback == null) return;

		var oldEffective = ResolveEffectiveValue(dp);
		RecomputeCoerced(dp);
		FireChangeIfDifferent(dp, oldEffective, ResolveEffectiveValue(dp));
	}

	/// <summary>
	/// Re-evaluates the property's effective value. Values here are resolved on demand, so this
	/// re-runs coercion; there are no cached expression values to refresh.
	/// </summary>
	public void InvalidateProperty(DependencyProperty dp)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		CoerceValue(dp);
	}

	// Rebuilds the coerced-result cache for dp from the raw layer values.
	void RecomputeCoerced(DependencyProperty dp)
	{
		var coerce = dp.GetMetadata(GetType()).CoerceValueCallback;
		_coercedValues?.Remove(dp);
		if (coerce == null) return;

		var plain = ResolveEffectiveValue(dp);
		var coerced = coerce(this, plain);
		if (!Equals(coerced, plain))
			(_coercedValues ??= new Dictionary<DependencyProperty, object?>())[dp] = coerced;
	}

	// Every layer mutation: recompute coercion, notify on effective change, then signal the source change.
	void CommitLayerChange(DependencyProperty dp, object? oldEffective)
	{
		RecomputeCoerced(dp);
		FireChangeIfDifferent(dp, oldEffective, ResolveEffectiveValue(dp));
		NotifyValueSource(dp);
	}
}
