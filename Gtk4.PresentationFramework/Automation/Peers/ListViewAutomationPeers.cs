using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a <see cref="ListView"/>.</summary>
/// <remarks>The layer has no <c>GridView</c>, so the grid pattern WPF adds for one is absent.</remarks>
public class ListViewAutomationPeer : ListBoxAutomationPeer
{
	public ListViewAutomationPeer(ListView owner) : base(owner) { }

	protected override string GetClassNameCore() => "ListView";

	protected override ItemAutomationPeer CreateItemAutomationPeer(object item) =>
		new ListViewItemAutomationPeer(item, this);
}

/// <summary>The automation peer of an item of a <see cref="ListView"/>.</summary>
public class ListViewItemAutomationPeer : ListBoxItemAutomationPeer
{
	public ListViewItemAutomationPeer(object owner, ListViewAutomationPeer listViewAutomationPeer)
		: base(owner, listViewAutomationPeer) { }

	protected override string GetClassNameCore() => "ListViewItem";
}
