using System.Collections;
using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a <see cref="ListBox"/>.</summary>
public class ListBoxAutomationPeer : SelectorAutomationPeer
{
	public ListBoxAutomationPeer(ListBox owner) : base(owner) { }

	ListBox ListBox => (ListBox)Owner;

	protected override string GetClassNameCore() => "ListBox";

	protected override ItemAutomationPeer CreateItemAutomationPeer(object item) =>
		new ListBoxItemAutomationPeer(item, this);

	internal override bool CanSelectMultipleCore => ListBox.SelectionMode != SelectionMode.Single;

	internal override IEnumerable<object> SelectedItemsList()
	{
		if (ListBox.SelectionMode == SelectionMode.Single)
		{
			foreach (var one in base.SelectedItemsList()) yield return one;
			yield break;
		}
		foreach (var item in ListBox.SelectedItems)
			if (item != null) yield return item;
	}

	internal override void SelectOnly(object item)
	{
		if (ListBox.SelectionMode != SelectionMode.Single) ListBox.UnselectAll();
		ListBox.SelectedItem = item;
	}

	internal override void SetSelected(object item, bool selected) => ListBox.AutomationSetSelected(item, selected);

	internal override bool AffectsAccessible(DependencyProperty property) =>
		property == ListBox.SelectedIndexProperty || base.AffectsAccessible(property);
}

/// <summary>The automation peer of an item of a <see cref="ListBox"/>.</summary>
public class ListBoxItemAutomationPeer : SelectorItemAutomationPeer
{
	public ListBoxItemAutomationPeer(object owner, SelectorAutomationPeer selectorAutomationPeer)
		: base(owner, selectorAutomationPeer) { }

	protected override string GetClassNameCore() => "ListBoxItem";
}

/// <summary>Peer of the container the layer creates for a list item; reports the item as selected.</summary>
sealed class ListBoxItemWrapperAutomationPeer : FrameworkElementAutomationPeer
{
	internal ListBoxItemWrapperAutomationPeer(ListBoxItem owner) : base(owner) { }

	protected override string GetClassNameCore() => Owner.GetType().Name;

	internal override string ContentName() => PeerText.FromContentLiteral(((ListBoxItem)Owner).Content);

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.ListItem;

	internal override bool AffectsAccessible(DependencyProperty property) =>
		property == ListBoxItem.IsSelectedProperty || base.AffectsAccessible(property);

	internal override void PushState(Gtk.Widget widget) =>
		PeerAccessible.SetState(widget, Gtk.AccessibleState.Selected, ((ListBoxItem)Owner).IsSelected);
}
