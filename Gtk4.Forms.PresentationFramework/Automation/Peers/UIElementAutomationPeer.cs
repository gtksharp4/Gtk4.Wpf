using System.Collections.Generic;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a <see cref="UIElement"/>.</summary>
public partial class UIElementAutomationPeer : AutomationPeer
{
	public UIElementAutomationPeer(UIElement owner)
	{
		Owner = owner ?? throw new ArgumentNullException(nameof(owner));
	}

	/// <summary>The element this peer describes.</summary>
	public UIElement Owner { get; }

	/// <summary>Returns the peer already created for <paramref name="element"/>, or <c>null</c>.</summary>
	public static AutomationPeer? FromElement(UIElement element) => element?.ExistingAutomationPeer;

	/// <summary>Returns the peer of <paramref name="element"/>, creating and caching it on first use.</summary>
	public static AutomationPeer? CreatePeerForElement(UIElement element) => element?.GetAutomationPeer();

	public override object? GetPattern(PatternInterface patternInterface) => null;

	protected override string GetAutomationIdCore() => AutomationProperties.GetAutomationId(Owner) ?? string.Empty;

	protected override string GetNameCore() => AutomationProperties.GetName(Owner) ?? string.Empty;

	protected override string GetHelpTextCore() => AutomationProperties.GetHelpText(Owner) ?? string.Empty;

	protected override string GetClassNameCore() => Owner.GetType().Name;

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Custom;

	protected override AutomationOrientation GetOrientationCore() => AutomationOrientation.None;

	protected override bool IsEnabledCore() => Owner.IsEnabled;

	protected override bool IsKeyboardFocusableCore() => Owner.Focusable;

	protected override bool HasKeyboardFocusCore() => Owner.IsKeyboardFocused;

	protected override bool IsOffscreenCore() => !Owner.IsVisible;

	protected override bool IsContentElementCore() => true;

	protected override bool IsControlElementCore() => true;

	protected override bool IsPasswordCore() => false;

	protected override bool IsRequiredForFormCore() => AutomationProperties.GetIsRequiredForForm(Owner);

	protected override AutomationLiveSetting GetLiveSettingCore() => AutomationProperties.GetLiveSetting(Owner);

	protected override AutomationPeer? GetLabeledByCore() =>
		AutomationProperties.GetLabeledBy(Owner) is UIElement label ? CreatePeerForElement(label) : null;

	protected override void SetFocusCore()
	{
		if (!Owner.Focus())
			throw new InvalidOperationException("Setting focus on the element failed.");
	}

	protected override List<AutomationPeer>? GetChildrenCore()
	{
		var result = new List<AutomationPeer>();
		Collect(Owner, result);
		return result.Count == 0 ? null : result;
	}

	// An element without a peer is transparent: its children are reported in its place.
	internal static void Collect(UIElement parent, List<AutomationPeer> into)
	{
		foreach (var child in parent.VisualChildren)
		{
			var peer = child.GetAutomationPeer();
			if (peer != null) into.Add(peer);
			else Collect(child, into);
		}
	}

	internal override AutomationPeer? ResolveParent()
	{
		for (var e = Owner.VisualParent as UIElement; e != null; e = e.VisualParent as UIElement)
		{
			var peer = e.GetAutomationPeer();
			if (peer != null) return peer;
		}
		return base.ResolveParent();
	}

	protected override Rect GetBoundingRectangleCore() => PeerGeometry.Bounds(Owner);

	protected override Point GetClickablePointCore()
	{
		var r = GetBoundingRectangle();
		return r.IsEmpty ? new Point(double.NaN, double.NaN) : new Point(r.X + r.Width / 2, r.Y + r.Height / 2);
	}
}
