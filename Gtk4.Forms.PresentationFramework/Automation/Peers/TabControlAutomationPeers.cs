using System.Collections.Generic;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a <see cref="TabControl"/>.</summary>
public class TabControlAutomationPeer : SelectorAutomationPeer
{
	public TabControlAutomationPeer(TabControl owner) : base(owner) { }

	protected override string GetClassNameCore() => "TabControl";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Tab;

	protected override AutomationOrientation GetOrientationCore() => AutomationOrientation.Horizontal;

	internal override bool IsSelectionRequiredCore => true;

	protected override ItemAutomationPeer CreateItemAutomationPeer(object item) =>
		new TabItemAutomationPeer(item, this);

	// The tab items, then the content of the selected one, as WPF reports them.
	protected override List<AutomationPeer>? GetChildrenCore()
	{
		_ = Owner.Widget; // the tab containers exist once the widget does
		var result = base.GetChildrenCore() ?? new List<AutomationPeer>();
		var control = (TabControl)Owner;
		if (control.SelectedItem != null
			&& control.ContainerAt(control.Items.IndexOf(control.SelectedItem)) is { } tab)
			UIElementAutomationPeer.Collect(tab, result);
		return result.Count == 0 ? null : result;
	}
}

/// <summary>The automation peer of an item of a <see cref="TabControl"/>.</summary>
public class TabItemAutomationPeer : SelectorItemAutomationPeer
{
	public TabItemAutomationPeer(object owner, TabControlAutomationPeer tabControlAutomationPeer)
		: base(owner, tabControlAutomationPeer) { }

	protected override string GetClassNameCore() => "TabItem";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.TabItem;
}

/// <summary>Peer of the <see cref="TabItem"/> the layer creates for a tab; it names the tab by its header.</summary>
sealed class TabItemWrapperAutomationPeer : FrameworkElementAutomationPeer
{
	internal TabItemWrapperAutomationPeer(TabItem owner) : base(owner) { }

	protected override string GetClassNameCore() => "TabItem";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.TabItem;

	internal override string ContentName() => PeerText.FromContentLiteral(((TabItem)Owner).Header);

	// The page box is not the tab; GTK's notebook owns the tab role.
	internal override Gtk.AccessibleRole? AccessibleRole => null;
}
