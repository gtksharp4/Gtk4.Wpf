using System.Collections.Generic;

namespace System.Windows.Data;

/// <summary>
/// Partial class that extends <see cref="BindingEngine"/> with clear-binding helpers
/// called by <see cref="BindingOperations"/>.
/// </summary>
internal static partial class BindingEngine
{
	/// <summary>
	/// Deactivates and removes the simple-binding record for <paramref name="dp"/>
	/// on <paramref name="target"/>, and removes its expression entry.
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

		if (_expressions.TryGetValue(target, out var exprMap))
			exprMap.Remove(dp);
	}

	/// <summary>
	/// Deactivates and removes ALL simple-binding records on <paramref name="target"/>,
	/// clearing the expression map too.
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

		// Also clear multi and priority bindings.
		MultiBindingEngine.ClearAll(target);
	}
}
