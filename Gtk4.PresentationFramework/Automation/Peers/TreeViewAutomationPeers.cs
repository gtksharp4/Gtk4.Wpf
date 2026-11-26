using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a <see cref="TreeView"/>; its children are the peers of its nodes.</summary>
public class TreeViewAutomationPeer : ItemsControlAutomationPeer, ISelectionProvider
{
	public TreeViewAutomationPeer(TreeView owner) : base(owner) { }

	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.Selection ? this : base.GetPattern(patternInterface);

	protected override string GetClassNameCore() => "TreeView";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Tree;

	protected override ItemAutomationPeer CreateItemAutomationPeer(object item) =>
		new TreeViewDataItemAutomationPeer(item, this);

	protected override List<AutomationPeer>? GetChildrenCore() => PeerTree.Containers((TreeView)Owner);

	bool ISelectionProvider.CanSelectMultiple => false;

	bool ISelectionProvider.IsSelectionRequired => false;

	IRawElementProviderSimple[] ISelectionProvider.GetSelection() =>
		((TreeView)Owner).SelectedItem is TreeViewItem item && item.GetAutomationPeer() is { } peer
			? new[] { ProviderFromPeer(peer) } : new IRawElementProviderSimple[0];

	internal override void OwnerPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (e.Property == TreeView.SelectedItemProperty && e.NewValue is TreeViewItem item)
			item.GetAutomationPeer()?.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementSelected);
	}
}

/// <summary>Peer of a data item of a <see cref="TreeView"/>; the layer reports the node containers instead.</summary>
sealed class TreeViewDataItemAutomationPeer : ItemAutomationPeer
{
	internal TreeViewDataItemAutomationPeer(object item, ItemsControlAutomationPeer parent) : base(item, parent) { }

	protected override string GetClassNameCore() => "TreeViewItem";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.TreeItem;
}
