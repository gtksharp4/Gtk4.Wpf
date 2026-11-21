using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Threading;

namespace System.Windows.Automation.Peers;

/// <summary>
/// The base class of every automation peer: the object model UI Automation clients read.
/// Subclasses override the <c>*Core</c> members; the public members call them.
/// </summary>
public abstract partial class AutomationPeer : DispatcherObject
{
	AutomationPeer? _parent;
	AutomationPeer? _eventsSource;
	IRawElementProviderSimple? _provider;

	protected AutomationPeer() { }

	/// <summary>The peer that events raised on this peer are reported for.</summary>
	public AutomationPeer? EventsSource
	{
		get => _eventsSource;
		set => _eventsSource = value;
	}

	/// <summary>Returns the object that supports <paramref name="patternInterface"/>, or <c>null</c>.</summary>
	public abstract object? GetPattern(PatternInterface patternInterface);

	// ── Tree ─────────────────────────────────────────────────────────────────────

	/// <summary>Returns the child peers. The list is rebuilt on every call.</summary>
	public List<AutomationPeer>? GetChildren()
	{
		var children = GetChildrenCore();
		if (children != null)
			foreach (var child in children)
				child._parent = this;
		return children;
	}

	/// <summary>Returns the parent peer, or <c>null</c> for a root.</summary>
	public AutomationPeer? GetParent() => ResolveParent();

	internal virtual AutomationPeer? ResolveParent() => _parent;

	internal void SetParent(AutomationPeer? parent) => _parent = parent;

	/// <summary>Discards cached state and tells listening clients the structure changed.</summary>
	public void InvalidatePeer() => RaiseStructureChanged();

	// ── Public wrappers over the Core members ────────────────────────────────────

	public string GetName() => GetNameCore();

	public string GetAutomationId() => GetAutomationIdCore();

	public string GetClassName() => GetClassNameCore();

	public string GetHelpText() => GetHelpTextCore();

	public string GetAcceleratorKey() => GetAcceleratorKeyCore();

	public string GetAccessKey() => GetAccessKeyCore();

	public string GetItemStatus() => GetItemStatusCore();

	public string GetItemType() => GetItemTypeCore();

	public string GetLocalizedControlType() => GetLocalizedControlTypeCore();

	public AutomationControlType GetAutomationControlType() => GetAutomationControlTypeCore();

	public Rect GetBoundingRectangle() => GetBoundingRectangleCore();

	public Point GetClickablePoint() => GetClickablePointCore();

	public AutomationPeer? GetLabeledBy() => GetLabeledByCore();

	public AutomationOrientation GetOrientation() => GetOrientationCore();

	public AutomationLiveSetting GetLiveSetting() => GetLiveSettingCore();

	public int GetPositionInSet() => GetPositionInSetCore();

	public int GetSizeOfSet() => GetSizeOfSetCore();

	public int GetLevel() => GetLevelCore();

	public bool HasKeyboardFocus() => HasKeyboardFocusCore();

	public bool IsContentElement() => IsContentElementCore();

	public bool IsControlElement() => IsControlElementCore();

	public bool IsDialog() => IsDialogCore();

	public bool IsEnabled() => IsEnabledCore();

	public bool IsKeyboardFocusable() => IsKeyboardFocusableCore();

	public bool IsOffscreen() => IsOffscreenCore();

	public bool IsPassword() => IsPasswordCore();

	public bool IsRequiredForForm() => IsRequiredForFormCore();

	public void SetFocus() => SetFocusCore();

	/// <summary>Returns the peer at <paramref name="point"/>, or <c>null</c>.</summary>
	public AutomationPeer? GetPeerFromPoint(Point point) => GetPeerFromPointCore(point);

	// ── Provider bridge ──────────────────────────────────────────────────────────

	/// <summary>Returns the provider that exposes <paramref name="peer"/> to clients.</summary>
	protected internal IRawElementProviderSimple ProviderFromPeer(AutomationPeer peer)
	{
		if (peer == null) throw new ArgumentNullException(nameof(peer));
		return peer._provider ??= new PeerProvider(peer);
	}

	/// <summary>Returns the peer behind <paramref name="provider"/>, or <c>null</c>.</summary>
	protected internal AutomationPeer? PeerFromProvider(IRawElementProviderSimple provider) =>
		(provider as PeerProvider)?.Peer;
}
