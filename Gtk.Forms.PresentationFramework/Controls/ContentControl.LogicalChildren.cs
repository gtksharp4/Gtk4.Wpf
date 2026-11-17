using System.Collections;

namespace System.Windows.Controls;

/// <summary>
/// Implements <see cref="ILogicalChildren"/> for <see cref="ContentControl"/>:
/// the logical child is <see cref="Content"/> when it is a <see cref="UIElement"/>.
/// </summary>
public partial class ContentControl : ILogicalChildren
{
	IEnumerable ILogicalChildren.LogicalChildrenCore
	{
		get
		{
			if (Content is UIElement child)
				yield return child;
		}
	}
}
