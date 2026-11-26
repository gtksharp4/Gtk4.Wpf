namespace System.Windows.Documents;

public abstract partial class Inline
{
	/// <summary>Gets or sets the flow direction of this inline (default LeftToRight).</summary>
	public FlowDirection FlowDirection
	{
		get => GetFlowDirection();
		set => SetFlowDirection(value);
	}

	internal void AppendWrappedMarkup(System.Text.StringBuilder sb) => AppendWrappedMarkup(sb, AppendMarkup);
}
