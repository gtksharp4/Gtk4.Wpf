using System;
using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Dynamic-resource value layer (D7) for <see cref="DependencyObject"/>.
/// Sits below the trigger layer and above the style layer, the same relative slot
/// as WPF's deferred-reference layer: a local value always wins, and when the local
/// value is cleared this layer re-surfaces.
/// </summary>
public partial class DependencyObject
{
	readonly Dictionary<DependencyProperty, object?> _dynResValues =
		new Dictionary<DependencyProperty, object?>();

	/// <summary>
	/// D7: Writes the resolved resource value (raw; coercion is applied on top) into the
	/// dynamic-resource layer. Lower priority than local/trigger; higher than style/inherited/default.
	/// </summary>
	internal void SetDynResValue(DependencyProperty dp, object? value)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		var oldEffective = ResolveEffectiveValue(dp);
		_dynResValues[dp] = value;
		CommitLayerChange(dp, oldEffective);
	}

	/// <summary>
	/// D7: Removes the dynamic-resource entry for <paramref name="dp"/> and fires change
	/// notifications.  Called when <see cref="FrameworkElement.ClearResourceReference"/> detaches
	/// the live link.
	/// </summary>
	internal void ClearDynResValue(DependencyProperty dp)
	{
		if (dp == null) return;
		if (!_dynResValues.ContainsKey(dp)) return;

		var oldEffective = ResolveEffectiveValue(dp);
		_dynResValues.Remove(dp);
		CommitLayerChange(dp, oldEffective);
	}
}
