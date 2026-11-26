using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a <see cref="Menu"/>.</summary>
public class MenuAutomationPeer : FrameworkElementAutomationPeer
{
	public MenuAutomationPeer(Menu owner) : base(owner) { }

	protected override string GetClassNameCore() => "Menu";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.MenuBar;

	protected override AutomationOrientation GetOrientationCore() => AutomationOrientation.Horizontal;

	protected override List<AutomationPeer>? GetChildrenCore() => PeerTree.Containers((Menu)Owner);
}

/// <summary>The automation peer of a <see cref="MenuItem"/>.</summary>
public class MenuItemAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider, IExpandCollapseProvider,
	IToggleProvider
{
	public MenuItemAutomationPeer(MenuItem owner) : base(owner) { }

	MenuItem Item => (MenuItem)Owner;

	// As in WPF: Invoke without sub-items, ExpandCollapse with them, Toggle when checkable.
	public override object? GetPattern(PatternInterface patternInterface)
	{
		switch (patternInterface)
		{
			case PatternInterface.Invoke: return Item.HasSubItems ? null : this;
			case PatternInterface.ExpandCollapse: return Item.HasSubItems ? this : null;
			case PatternInterface.Toggle: return Item.IsCheckable ? this : null;
			default: return base.GetPattern(patternInterface);
		}
	}

	protected override string GetClassNameCore() => "MenuItem";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.MenuItem;

	protected override string GetAcceleratorKeyCore()
	{
		var key = base.GetAcceleratorKeyCore();
		return key == string.Empty ? Item.InputGestureText ?? string.Empty : key;
	}

	internal override string ContentName() => PeerText.FromContent(Item.Header);

	protected override List<AutomationPeer>? GetChildrenCore()
	{
		var result = new List<AutomationPeer>();
		foreach (var child in Item.Items)
			if (child is MenuItem sub) PeerTree.AddPeerOf(sub, result);
		return result.Count == 0 ? null : result;
	}

	internal override Gtk.AccessibleRole? AccessibleRole =>
		Item.IsCheckable ? Gtk.AccessibleRole.MenuItemCheckbox : Gtk.AccessibleRole.MenuItem;

	void IInvokeProvider.Invoke()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		Dispatcher.BeginInvoke(Item.AutomationActivate);
	}

	ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState =>
		Item.IsSubmenuShown ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;

	void IExpandCollapseProvider.Expand()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		Item.AutomationSetSubmenuShown(true);
	}

	void IExpandCollapseProvider.Collapse()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		Item.AutomationSetSubmenuShown(false);
	}

	ToggleState IToggleProvider.ToggleState => Item.IsChecked ? ToggleState.On : ToggleState.Off;

	void IToggleProvider.Toggle()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		Item.IsChecked = !Item.IsChecked;
	}

	internal override bool AffectsAccessible(DependencyProperty property) =>
		property == MenuItem.IsCheckedProperty || property == MenuItem.IsCheckableProperty
		|| property == MenuItem.InputGestureTextProperty
		|| base.AffectsAccessible(property);

	internal override void PushState(Gtk.Widget widget)
	{
		if (Item.IsCheckable) PeerAccessible.SetState(widget, Gtk.AccessibleState.Checked, Item.IsChecked);
	}

	internal override void OwnerPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (e.Property == MenuItem.IsCheckedProperty)
			RaisePropertyChangedEvent(TogglePatternIdentifiers.ToggleStateProperty,
				(bool)e.OldValue! ? ToggleState.On : ToggleState.Off,
				(bool)e.NewValue! ? ToggleState.On : ToggleState.Off);
	}
}
