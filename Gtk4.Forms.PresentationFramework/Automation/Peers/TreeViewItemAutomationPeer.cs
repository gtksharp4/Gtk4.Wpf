using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a <see cref="TreeViewItem"/>.</summary>
public class TreeViewItemAutomationPeer : ItemsControlAutomationPeer, IExpandCollapseProvider, ISelectionItemProvider
{
	public TreeViewItemAutomationPeer(TreeViewItem owner) : base(owner) { }

	TreeViewItem Node => (TreeViewItem)Owner;

	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.ExpandCollapse || patternInterface == PatternInterface.SelectionItem
			? this : base.GetPattern(patternInterface);

	protected override string GetClassNameCore() => "TreeViewItem";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.TreeItem;

	internal override string ContentName() => PeerText.FromContentLiteral(Node.Header);

	protected override ItemAutomationPeer CreateItemAutomationPeer(object item) =>
		new TreeViewDataItemAutomationPeer(item, this);

	protected override List<AutomationPeer>? GetChildrenCore() => PeerTree.Containers(Node);

	ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState =>
		Node.Items.Count == 0 ? ExpandCollapseState.LeafNode
			: Node.IsExpanded ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;

	void IExpandCollapseProvider.Expand()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		if (Node.Items.Count == 0) throw new InvalidOperationException("A leaf node cannot be expanded.");
		Node.IsExpanded = true;
	}

	void IExpandCollapseProvider.Collapse()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		if (Node.Items.Count == 0) throw new InvalidOperationException("A leaf node cannot be collapsed.");
		Node.IsExpanded = false;
	}

	TreeView? Tree
	{
		get
		{
			for (UIElement? p = Node.LogicalParent; p != null; p = p.LogicalParent)
				if (p is TreeView tree) return tree;
			return null;
		}
	}

	bool ISelectionItemProvider.IsSelected => Node.IsSelected;

	IRawElementProviderSimple ISelectionItemProvider.SelectionContainer =>
		Tree?.GetAutomationPeer() is { } peer ? ProviderFromPeer(peer) : null!;

	void ISelectionItemProvider.Select()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		if (Tree is { } tree) tree.SelectedItem = Node;
		else Node.IsSelected = true;
	}

	// A tree selects one node: adding throws only while a different node is selected.
	void ISelectionItemProvider.AddToSelection()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		var selected = Tree?.SelectedItem;
		if (selected != null && !ReferenceEquals(selected, Node))
			throw new InvalidOperationException("A tree allows a single selection.");
		if (Tree is { } tree) tree.SelectedItem = Node;
		else Node.IsSelected = true;
	}

	void ISelectionItemProvider.RemoveFromSelection()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		if (Node.IsSelected) Node.IsSelected = false;
	}

	internal override bool AffectsAccessible(DependencyProperty property) =>
		property == TreeViewItem.IsExpandedProperty || property == TreeViewItem.IsSelectedProperty
		|| base.AffectsAccessible(property);

	internal override void PushState(Gtk.Widget widget)
	{
		PeerAccessible.SetState(widget, Gtk.AccessibleState.Selected, Node.IsSelected);
		if (Node.Items.Count > 0) PeerAccessible.SetState(widget, Gtk.AccessibleState.Expanded, Node.IsExpanded);
		int level = 1;
		for (UIElement? p = Node.LogicalParent; p != null; p = p.LogicalParent)
			if (p is TreeViewItem) level++;
		PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.Level, level);
	}

	internal override void OwnerPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (e.Property == TreeViewItem.IsExpandedProperty)
			RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
				(bool)e.OldValue! ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
				(bool)e.NewValue! ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
		else if (e.Property == TreeViewItem.IsSelectedProperty)
			RaisePropertyChangedEvent(SelectionItemPatternIdentifiers.IsSelectedProperty, e.OldValue, e.NewValue);
	}
}
