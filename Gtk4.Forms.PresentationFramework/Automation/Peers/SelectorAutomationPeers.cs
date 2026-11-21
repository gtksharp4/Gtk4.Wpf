using System.Collections;
using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a <see cref="Selector"/>.</summary>
public abstract partial class SelectorAutomationPeer : ItemsControlAutomationPeer, ISelectionProvider
{
	protected SelectorAutomationPeer(Selector owner) : base(owner) { }

	internal Selector Selector => (Selector)Owner;

	internal DependencyProperty SelectedItemProperty => Selector.SelectedItemDependencyProperty!;

	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.Selection ? this : base.GetPattern(patternInterface);

	internal virtual bool CanSelectMultipleCore => false;

	bool ISelectionProvider.CanSelectMultiple => CanSelectMultipleCore;

	internal virtual bool IsSelectionRequiredCore => false;

	bool ISelectionProvider.IsSelectionRequired => IsSelectionRequiredCore;

	internal virtual IEnumerable<object> SelectedItemsList()
	{
		var item = Selector.GetValue(SelectedItemProperty);
		if (item != null) yield return item;
	}

	IRawElementProviderSimple[] ISelectionProvider.GetSelection()
	{
		var result = new List<IRawElementProviderSimple>();
		foreach (var item in SelectedItemsList())
			result.Add(ProviderFromPeer(ItemPeerFor(item)));
		return result.ToArray();
	}

	internal virtual void SelectOnly(object item) => Selector.SetValue(SelectedItemProperty, item);

	internal virtual void SetSelected(object item, bool selected)
	{
		if (selected) Selector.SetValue(SelectedItemProperty, item);
		else if (Equals(Selector.GetValue(SelectedItemProperty), item)) Selector.SetValue(SelectedItemProperty, null);
	}

	internal override bool AffectsAccessible(DependencyProperty property) =>
		property == SelectedItemProperty || base.AffectsAccessible(property);

	// Marks the containers GTK created for the items as selected or not.
	internal override void PushState(Gtk.Widget widget)
	{
		var selected = new HashSet<object>(SelectedItemsList());
		var items = Selector.Items;
		for (int i = 0; i < items.Count; i++)
		{
			var container = Selector.ContainerAt(i);
			if (container != null && container.HasBackingWidget)
				PeerAccessible.SetState(container.Widget, Gtk.AccessibleState.Selected,
					items[i] != null && selected.Contains(items[i]!));
		}
	}

}

/// <summary>The automation peer of an item of a <see cref="Selector"/>.</summary>
public abstract class SelectorItemAutomationPeer : ItemAutomationPeer, ISelectionItemProvider
{
	protected SelectorItemAutomationPeer(object owner, SelectorAutomationPeer selectorAutomationPeer)
		: base(owner, selectorAutomationPeer) { }

	SelectorAutomationPeer Selector => (SelectorAutomationPeer)ItemsControlAutomationPeer;

	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.SelectionItem ? this : base.GetPattern(patternInterface);

	bool ISelectionItemProvider.IsSelected => IsItemSelected;

	bool IsItemSelected
	{
		get
		{
			foreach (var item in Selector.SelectedItemsList())
				if (Equals(item, Item)) return true;
			return false;
		}
	}

	IRawElementProviderSimple ISelectionItemProvider.SelectionContainer => ProviderFromPeer(Selector);

	void ISelectionItemProvider.Select()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		Selector.SelectOnly(Item);
	}

	void ISelectionItemProvider.AddToSelection()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		if (!Selector.CanSelectMultipleCore && !IsItemSelected
			&& Selector.SelectedItemsList().GetEnumerator().MoveNext())
			throw new InvalidOperationException("The container does not allow multiple selection.");
		Selector.SetSelected(Item, true);
	}

	void ISelectionItemProvider.RemoveFromSelection()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		Selector.SetSelected(Item, false);
	}
}
