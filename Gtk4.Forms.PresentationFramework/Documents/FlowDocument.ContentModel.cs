namespace System.Windows.Documents;

/// <summary>
/// FlowDocument is a <see cref="FrameworkContentElement"/>: it carries a name, style, resources and data context,
/// and its blocks inherit from it. The existing CLR members (string FontFamily, NaN-means-inherit FontSize,
/// nullable TextAlignment, PagePadding, FlowDirection) are kept; the new members are real DPs.
/// </summary>
public partial class FlowDocument : FrameworkContentElement
{
	/// <summary>Creates an empty document.</summary>
	public FlowDocument()
	{
		Blocks = new BlockCollection(this);
		Blocks.Changed += () =>
		{
			ViewChanged?.Invoke();
			ContentChanged?.Invoke();
		};
	}

	/// <summary>Creates a document holding <paramref name="block"/>.</summary>
	public FlowDocument(Block block) : this()
	{
		if (block == null) throw new ArgumentNullException(nameof(block));
		Blocks.Add(block);
	}
}
