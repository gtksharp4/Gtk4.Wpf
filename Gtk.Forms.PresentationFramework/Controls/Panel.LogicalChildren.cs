using System.Collections;

namespace System.Windows.Controls;

/// <summary>
/// Implements <see cref="ILogicalChildren"/> for <see cref="Panel"/>:
/// the logical children are the panel's <see cref="UIElementCollection"/>.
/// </summary>
public abstract partial class Panel : ILogicalChildren
{
	IEnumerable ILogicalChildren.LogicalChildrenCore => Children;
}
