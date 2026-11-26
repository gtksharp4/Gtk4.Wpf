using System.Collections;

namespace System.Windows.Controls;

public abstract partial class Panel
{
	/// <summary>The logical children are the panel's <see cref="UIElementCollection"/>.</summary>
	protected internal override IEnumerator LogicalChildren => ((IEnumerable)Children).GetEnumerator();
}
