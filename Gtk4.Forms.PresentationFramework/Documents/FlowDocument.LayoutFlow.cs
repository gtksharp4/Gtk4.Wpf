namespace System.Windows.Documents;

public partial class FlowDocument
{
	/// <summary>The direction pushed into each block's markup, as <see cref="BuildMarkup"/> does.</summary>
	internal FlowDirection? FlowDirectionForLayout => ExplicitFlowDirection;
}
