using System.Collections.Generic;

namespace System.Windows.Data;

internal static partial class BindingEngine
{
	/// <summary>A snapshot of the live expressions on <paramref name="target"/>.</summary>
	internal static List<BindingExpressionBase> ExpressionsOf(DependencyObject target) =>
		_expressions.TryGetValue(target, out var map)
			? new List<BindingExpressionBase>(map.Values) : new List<BindingExpressionBase>();
}
