using System.Collections;
using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a <see cref="Selector"/>.</summary>
public class SelectorAutomationPeer : ItemsControlAutomationPeer, ISelectionProvider
{
	public SelectorAutomationPeer(Selector owner) : base(owner) { }

	internal Selector Selector => (Selector)Owner;

	internal DependencyProperty SelectedItemProperty => Selector.SelectedItemDependencyProperty!;

	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.Selection ? this : base.GetPattern(patternInterface);

	public virtual bool CanSelectMultiple => false;

	public bool IsSelectionRequired => false;

	internal virtual IEnumerable<object> SelectedItemsList()
	{
		var item = Selector.GetValue(SelectedItemProperty);
		if (item != null) yield return item;
	}

	public IRawElementProviderSimple[] GetSelection()
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

	internal override void OwnerPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (e.Property == SelectedItemProperty)
			RaiseAutomationEvent(AutomationEvents.SelectionPatternOnInvalidated);
	}
}

/// <summary>The automation peer of an item of a <see cref="Selector"/>.</summary>
public class SelectorItemAutomationPeer : ItemAutomationPeer, ISelectionItemProvider
{
	public SelectorItemAutomationPeer(object owner, SelectorAutomationPeer selectorAutomationPeer)
		: base(owner, selectorAutomationPeer) { }

	SelectorAutomationPeer Selector => (SelectorAutomationPeer)ItemsControlAutomationPeer;

	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.SelectionItem ? this : base.GetPattern(patternInterface);

	public bool IsSelected
	{
		get
		{
			foreach (var item in Selector.SelectedItemsList())
				if (Equals(item, Item)) return true;
			return false;
		}
	}

	public IRawElementProviderSimple SelectionContainer => ProviderFromPeer(Selector);

	public void Select()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		Selector.SelectOnly(Item);
	}

	public void AddToSelection()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		if (!Selector.CanSelectMultiple && Selector.SelectedItemsList().GetEnumerator().MoveNext() && !IsSelected)
			throw new InvalidOperationException("The container does not allow multiple selection.");
		Selector.SetSelected(Item, true);
	}

	public void RemoveFromSelection()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		Selector.SetSelected(Item, false);
	}
}
