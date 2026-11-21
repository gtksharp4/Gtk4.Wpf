namespace System.Windows.Documents;

public abstract partial class Block
{
	/// <summary>Gets or sets the flow direction of this block (default LeftToRight).</summary>
	public FlowDirection FlowDirection
	{
		get => GetFlowDirection();
		set => SetFlowDirection(value);
	}

	internal void AppendWrappedMarkup(System.Text.StringBuilder sb) => AppendWrappedMarkup(sb, AppendMarkup);

	/// <summary>
	/// Appends this block for a container whose explicit direction is <paramref name="inherited"/>: the block's own
	/// explicit direction wins. Containers pass the direction down to their paragraphs instead of wrapping them.
	/// </summary>
	internal void AppendBlockMarkup(System.Text.StringBuilder sb, FlowDirection? inherited) =>
		AppendFlowMarkup(sb, ExplicitFlowDirection ?? inherited);

	/// <summary>Appends this block with the effective direction (<c>null</c> when none is set anywhere).</summary>
	internal virtual void AppendFlowMarkup(System.Text.StringBuilder sb, FlowDirection? effective) =>
		AppendParagraph(sb, effective, AppendMarkup);
}
