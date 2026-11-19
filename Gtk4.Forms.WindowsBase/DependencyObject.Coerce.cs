using System;
using System.Collections.Generic;

namespace System.Windows;

public partial class DependencyObject
{
	// The un-coerced value last passed to SetValue. The local store keeps the coerced result,
	// so CoerceValue needs this to re-run coercion (and to restore a relaxed value).
	Dictionary<DependencyProperty, object?>? _baseValues;

	// Coerced result for a property with no local value (e.g. IsEnabled forced false by a
	// parent). Sits above every other layer, as coercion does in WPF.
	Dictionary<DependencyProperty, object?>? _coercedValues;

	/// <summary>
	/// Re-runs the property's <see cref="PropertyMetadata.CoerceValueCallback"/> against the current
	/// base value. The change callback and <c>OnPropertyChanged</c> run only if the effective value changes.
	/// </summary>
	public void CoerceValue(DependencyProperty dp)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		var coerce = dp.GetMetadata(GetType()).CoerceValueCallback;
		if (coerce == null) return;

		var oldEffective = ResolveEffectiveValue(dp);
		if (_baseValues != null && _baseValues.TryGetValue(dp, out var baseValue))
		{
			_localValues[dp] = coerce(this, baseValue);
		}
		else
		{
			_coercedValues?.Remove(dp);
			var plain = ResolveEffectiveValue(dp);
			var coerced = coerce(this, plain);
			if (!Equals(coerced, plain))
				(_coercedValues ??= new Dictionary<DependencyProperty, object?>())[dp] = coerced;
		}
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

	void RecordBaseValue(DependencyProperty dp, PropertyMetadata meta, object? rawValue)
	{
		// A fresh local value supersedes any coercion-only result.
		_coercedValues?.Remove(dp);
		if (meta.CoerceValueCallback != null)
			(_baseValues ??= new Dictionary<DependencyProperty, object?>())[dp] = rawValue;
	}
}
