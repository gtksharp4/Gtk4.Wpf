namespace System.Windows.Documents;

public partial class FlowDocument
{
	/// <summary>Tells the hosting viewers to re-render (an inline's content changed).</summary>
	internal void NotifyViewChanged() => ViewChanged?.Invoke();
}
