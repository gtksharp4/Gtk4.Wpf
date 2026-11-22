using System.Collections;

namespace System.Windows.Controls;

public partial class ContentControl
{
	/// <summary>The logical child is <see cref="Content"/> when it is a <see cref="UIElement"/>.</summary>
	protected internal override IEnumerator? LogicalChildren =>
		Content is UIElement child ? new[] { child }.GetEnumerator() : base.LogicalChildren;
}
