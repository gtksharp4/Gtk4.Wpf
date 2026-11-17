using System.Collections.Generic;

namespace System.Windows.Data;

/// <summary>
/// Partial class that extends <see cref="BindingEngine"/> with clear-binding helpers
/// called by <see cref="BindingOperations"/>.
/// </summary>
internal static partial class BindingEngine
{
	/// <summary>
	/// Deactivates and removes the binding record (simple, multi, or priority) for
	/// <paramref name="dp"/> on <paramref name="target"/>, and removes its expression entry.
	/// This is the single authority for per-DP clear; <see cref="BindingOperations.ClearBinding"/>
	/// routes here and does not duplicate the sub-engine calls.
	/// </summary>
	internal static void ClearBinding(FrameworkElement target, DependencyProperty dp)
	{
		if (_bindings.TryGetValue(target, out var list))
		{
			int idx = list.FindIndex(r => r.TargetProperty == dp);
			if (idx >= 0)
			{
				list[idx].Deactivate();
				list.RemoveAt(idx);
			}
		}

		// Clear multi/priority records so their INPC handlers don't survive.
		MultiBindingEngine.ClearForDp(target, dp);
		PriorityBindingEngine.ClearForDp(target, dp);

		if (_expressions.TryGetValue(target, out var exprMap))
			exprMap.Remove(dp);
	}

	/// <summary>
	/// Deactivates and removes ALL bindings (simple, multi, and priority) on
	/// <paramref name="target"/>, clearing the expression map too.
	/// </summary>
	internal static void ClearAllBindings(FrameworkElement target)
	{
		if (_bindings.TryGetValue(target, out var list))
		{
			foreach (var r in list) r.Deactivate();
			list.Clear();
		}

		if (_expressions.TryGetValue(target, out var exprMap))
			exprMap.Clear();

		MultiBindingEngine.ClearAll(target);
		PriorityBindingEngine.ClearAll(target);
	}
}
