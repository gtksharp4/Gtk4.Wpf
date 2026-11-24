using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of an <see cref="Expander"/>.</summary>
public class ExpanderAutomationPeer : FrameworkElementAutomationPeer, IExpandCollapseProvider
{
	public ExpanderAutomationPeer(Expander owner) : base(owner) { }

	Expander Expander => (Expander)Owner;

	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.ExpandCollapse ? this : base.GetPattern(patternInterface);

	protected override string GetClassNameCore() => "Expander";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Group;

	internal override string ContentName() => PeerText.FromContentLiteral(Expander.Header);

	ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState =>
		Expander.IsExpanded ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;

	void IExpandCollapseProvider.Expand()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		Expander.IsExpanded = true;
	}

	void IExpandCollapseProvider.Collapse()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		Expander.IsExpanded = false;
	}

	// The expander is a toggle for its header, so GTK keeps the button role it has.
	internal override Gtk.AccessibleRole? AccessibleRole => null;

	internal override bool AffectsAccessible(DependencyProperty property) =>
		property == Expander.IsExpandedProperty || base.AffectsAccessible(property);

	internal override void PushState(Gtk.Widget widget) =>
		PeerAccessible.SetState(widget, Gtk.AccessibleState.Expanded, Expander.IsExpanded);

	internal override void OwnerPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (e.Property == Expander.IsExpandedProperty)
			RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
				(bool)e.OldValue! ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
				(bool)e.NewValue! ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
	}
}

/// <summary>The automation peer of a <see cref="GroupBox"/>.</summary>
public class GroupBoxAutomationPeer : FrameworkElementAutomationPeer
{
	public GroupBoxAutomationPeer(GroupBox owner) : base(owner) { }

	protected override string GetClassNameCore() => "GroupBox";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Group;

	internal override string ContentName() => PeerText.FromContentLiteral(((GroupBox)Owner).Header);
}

/// <summary>The automation peer of a <see cref="StatusBar"/>.</summary>
public class StatusBarAutomationPeer : FrameworkElementAutomationPeer
{
	public StatusBarAutomationPeer(StatusBar owner) : base(owner) { }

	protected override string GetClassNameCore() => "StatusBar";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.StatusBar;

	protected override List<AutomationPeer>? GetChildrenCore() => PeerTree.Containers((StatusBar)Owner);
}

/// <summary>The automation peer of a <see cref="ToolBar"/>.</summary>
public class ToolBarAutomationPeer : FrameworkElementAutomationPeer
{
	public ToolBarAutomationPeer(ToolBar owner) : base(owner) { }

	protected override string GetClassNameCore() => "ToolBar";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.ToolBar;

	protected override AutomationOrientation GetOrientationCore() => AutomationOrientation.Horizontal;

	protected override List<AutomationPeer>? GetChildrenCore() => PeerTree.Containers((ToolBar)Owner);
}
