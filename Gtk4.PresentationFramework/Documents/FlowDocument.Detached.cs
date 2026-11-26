namespace System.Windows.Documents;

public partial class FlowDocument
{
	/// <summary>
	/// Set on the private container of a detached element (see <c>TextElement.EnsureTextContainer</c>): the text
	/// tree is that element's subtree and <see cref="Blocks"/> stays empty. Cleared when the element is adopted,
	/// which empties every pointer made on the container.
	/// </summary>
	internal object? DetachedRoot
	{
		get => _detachedRoot;
		set
		{
			_detachedRoot = value;
			InvalidateSymbols();
		}
	}

	object? _detachedRoot;
}
