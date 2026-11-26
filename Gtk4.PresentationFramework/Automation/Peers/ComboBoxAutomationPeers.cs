using System.Collections;
using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a <see cref="ComboBox"/>.</summary>
public class ComboBoxAutomationPeer : SelectorAutomationPeer, IValueProvider, IExpandCollapseProvider
{
	public ComboBoxAutomationPeer(ComboBox owner) : base(owner) { }

	ComboBox ComboBox => (ComboBox)Owner;

	// Value exists only for an editable box, as in WPF.
	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.ExpandCollapse
		|| (patternInterface == PatternInterface.Value && ComboBox.IsEditable)
			? this : base.GetPattern(patternInterface);

	protected override string GetClassNameCore() => "ComboBox";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.ComboBox;

	protected override ItemAutomationPeer CreateItemAutomationPeer(object item) =>
		new ComboBoxItemAutomationPeer(item, this);

	// The editable host box is a wrapper; the drop-down carries the accessible node.
	internal override Gtk.Widget Target(Gtk.Widget widget) => ComboBox.AccessibleTarget;

	bool IValueProvider.IsReadOnly => !IsEnabled() || ComboBox.IsReadOnly;

	string IValueProvider.Value => ComboBox.Text;

	void IValueProvider.SetValue(string value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));
		if (!IsEnabled()) throw new ElementNotEnabledException();
		ComboBox.Text = value;
	}

	ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState =>
		ComboBox.IsDropDownOpen ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;

	void IExpandCollapseProvider.Expand()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		ComboBox.IsDropDownOpen = true;
	}

	void IExpandCollapseProvider.Collapse()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		ComboBox.IsDropDownOpen = false;
	}

	internal override bool AffectsAccessible(DependencyProperty property) =>
		property == ComboBox.IsDropDownOpenProperty || base.AffectsAccessible(property);

	internal override void PushState(Gtk.Widget widget)
	{
		PeerAccessible.SetState(widget, Gtk.AccessibleState.Expanded, ComboBox.IsDropDownOpen);
		base.PushState(widget);
	}

	internal override void OwnerPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (e.Property == ComboBox.IsDropDownOpenProperty)
			RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
				(bool)e.OldValue! ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
				(bool)e.NewValue! ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
		else
			base.OwnerPropertyChanged(e);
	}
}

/// <summary>The automation peer of an item of a <see cref="ComboBox"/>.</summary>
public class ComboBoxItemAutomationPeer : SelectorItemAutomationPeer
{
	public ComboBoxItemAutomationPeer(object owner, ComboBoxAutomationPeer comboBoxAutomationPeer)
		: base(owner, comboBoxAutomationPeer) { }

	protected override string GetClassNameCore() => "ComboBoxItem";
}
