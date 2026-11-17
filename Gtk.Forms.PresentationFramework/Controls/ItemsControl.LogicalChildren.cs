using System.Collections;

namespace System.Windows.Controls;

/// <summary>
/// Implements <see cref="ILogicalChildren"/> for <see cref="ItemsControl"/>:
/// the logical children are the item containers currently generated.
/// </summary>
public partial class ItemsControl : ILogicalChildren
{
	IEnumerable ILogicalChildren.LogicalChildrenCore
	{
		get
		{
			foreach (var row in _rows)
				yield return row.Container;
		}
	}
}
