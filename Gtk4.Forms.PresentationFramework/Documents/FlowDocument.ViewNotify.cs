namespace System.Windows.Documents;

public partial class FlowDocument
{
	/// <summary>Tells the hosting viewers to re-render and the paginator to repaginate (the content changed).</summary>
	internal void NotifyContentEdited()
	{
		ViewChanged?.Invoke();
		ContentChanged?.Invoke();
	}
}
