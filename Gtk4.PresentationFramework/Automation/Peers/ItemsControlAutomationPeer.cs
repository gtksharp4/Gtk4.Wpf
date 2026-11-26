using System.Collections.Generic;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of an <see cref="ItemsControl"/>; its children are one peer per item.</summary>
public abstract class ItemsControlAutomationPeer : FrameworkElementAutomationPeer
{
	readonly Dictionary<object, ItemAutomationPeer> _itemPeers = new Dictionary<object, ItemAutomationPeer>();

	protected ItemsControlAutomationPeer(ItemsControl owner) : base(owner) { }

	public override object? GetPattern(PatternInterface patternInterface) => null;

	protected override string GetClassNameCore() => Owner.GetType().Name;

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.List;

	/// <summary>Creates the peer for one item.</summary>
	protected abstract ItemAutomationPeer CreateItemAutomationPeer(object item);

	/// <summary>Returns the cached peer of <paramref name="item"/>, creating it on first use.</summary>
	internal ItemAutomationPeer ItemPeerFor(object item)
	{
		if (!_itemPeers.TryGetValue(item, out var peer))
			_itemPeers[item] = peer = CreateItemAutomationPeer(item);
		return peer;
	}

	protected override List<AutomationPeer>? GetChildrenCore()
	{
		var items = ((ItemsControl)Owner).Items;
		var result = new List<AutomationPeer>(items.Count);
		var live = new HashSet<object>();
		foreach (var item in items)
			if (item != null && live.Add(item))
				result.Add(ItemPeerFor(item));
		foreach (var stale in new List<object>(_itemPeers.Keys))
			if (!live.Contains(stale))
				_itemPeers.Remove(stale);
		return result.Count == 0 ? null : result;
	}
}

/// <summary>The automation peer of one data item of an <see cref="ItemsControl"/>.</summary>
public abstract class ItemAutomationPeer : AutomationPeer
{
	protected ItemAutomationPeer(object item, ItemsControlAutomationPeer itemsControlPeer)
	{
		Item = item ?? throw new ArgumentNullException(nameof(item));
		ItemsControlAutomationPeer = itemsControlPeer ?? throw new ArgumentNullException(nameof(itemsControlPeer));
	}

	/// <summary>The data item.</summary>
	public object Item { get; }

	/// <summary>The peer of the control that holds the item.</summary>
	public ItemsControlAutomationPeer ItemsControlAutomationPeer { get; }

	internal ItemsControl ItemsControl => (ItemsControl)ItemsControlAutomationPeer.Owner;

	// The container, once the control has created one for the item.
	internal UIElement? Container => ItemsControl.ContainerAt(ItemsControl.Items.IndexOf(Item));

	internal override AutomationPeer? ResolveParent() => ItemsControlAutomationPeer;

	public override object? GetPattern(PatternInterface patternInterface) => null;

	protected override string GetClassNameCore() => "ListBoxItem";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.ListItem;

	// The name of the container's peer (its content's plain text), else the item itself.
	protected override string GetNameCore()
	{
		if (Item is DependencyObject itemObject && AutomationProperties.GetName(itemObject) is { Length: > 0 } itemName)
			return itemName;
		var container = Container;
		if (container != null)
		{
			var name = container.GetAutomationPeer()?.GetName();
			if (string.IsNullOrEmpty(name)) name = AutomationProperties.GetName(container);
			if (!string.IsNullOrEmpty(name)) return name!;
		}
		return PeerText.Plain(Item);
	}

	protected override string GetAutomationIdCore() =>
		Container is { } c ? AutomationProperties.GetAutomationId(c) ?? string.Empty : string.Empty;

	protected override string GetHelpTextCore() => string.Empty;

	protected override List<AutomationPeer>? GetChildrenCore() => null;

	protected override Rect GetBoundingRectangleCore() =>
		Container is { } c ? PeerGeometry.Bounds(c) : Rect.Empty;

	protected override Point GetClickablePointCore()
	{
		var r = GetBoundingRectangle();
		return r.IsEmpty ? new Point(double.NaN, double.NaN) : new Point(r.X + r.Width / 2, r.Y + r.Height / 2);
	}

	protected override AutomationOrientation GetOrientationCore() => AutomationOrientation.None;

	protected override bool IsEnabledCore() => ItemsControlAutomationPeer.IsEnabled();

	protected override bool IsOffscreenCore() => GetBoundingRectangle().IsEmpty;

	protected override bool IsKeyboardFocusableCore() => true;

	protected override bool HasKeyboardFocusCore() => Container?.IsKeyboardFocused ?? false;

	protected override bool IsContentElementCore() => true;

	protected override bool IsControlElementCore() => true;

	protected override bool IsPasswordCore() => false;

	protected override bool IsRequiredForFormCore() => false;

	protected override void SetFocusCore()
	{
		if (Container?.Focus() != true)
			throw new InvalidOperationException("Setting focus on the item failed.");
	}
}
