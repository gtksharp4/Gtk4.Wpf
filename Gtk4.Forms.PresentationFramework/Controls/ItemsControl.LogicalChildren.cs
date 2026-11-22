using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Controls;

public partial class ItemsControl
{
	/// <summary>The logical children are the item containers currently generated.</summary>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			var containers = new List<UIElement>();
			foreach (var row in _rows) containers.Add(row.Container);
			return containers.GetEnumerator();
		}
	}
}
