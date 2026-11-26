using System.Collections.Generic;
using System.Windows.Automation.Peers;

namespace System.Windows.Automation;

/// <summary>
/// The root of the in-process automation tree: the pane whose children are the application's visible windows.
/// Real UI Automation roots at the desktop; this layer has no other process to show.
/// </summary>
sealed class DesktopAutomationPeer : AutomationPeer
{
	internal static readonly DesktopAutomationPeer Instance = new DesktopAutomationPeer();

	DesktopAutomationPeer() { }

	public override object? GetPattern(PatternInterface patternInterface) => null;

	protected override string GetClassNameCore() => "#32769";

	protected override string GetNameCore() => "Desktop";

	protected override string GetAutomationIdCore() => string.Empty;

	protected override string GetHelpTextCore() => string.Empty;

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Pane;

	protected override Rect GetBoundingRectangleCore() => Rect.Empty;

	protected override Point GetClickablePointCore() => new Point(double.NaN, double.NaN);

	protected override AutomationOrientation GetOrientationCore() => AutomationOrientation.None;

	protected override bool HasKeyboardFocusCore() => false;

	protected override bool IsContentElementCore() => false;

	protected override bool IsControlElementCore() => true;

	protected override bool IsEnabledCore() => true;

	protected override bool IsKeyboardFocusableCore() => false;

	protected override bool IsOffscreenCore() => false;

	protected override bool IsPasswordCore() => false;

	protected override bool IsRequiredForFormCore() => false;

	protected override void SetFocusCore() { }

	protected override List<AutomationPeer>? GetChildrenCore()
	{
		var result = new List<AutomationPeer>();
		var windows = Application.Current?.Windows;
		if (windows != null)
			foreach (Window window in windows)
				if (window.Visibility == Visibility.Visible && window.GetAutomationPeer() is { } peer)
					result.Add(peer);
		return result.Count == 0 ? null : result;
	}

	protected override AutomationPeer? GetPeerFromPointCore(Point point)
	{
		var children = GetChildren();
		if (children != null)
			for (int i = children.Count - 1; i >= 0; i--)
			{
				var bounds = children[i].GetBoundingRectangle();
				if (PeerGeometry.Contains(bounds, point))
					return children[i].GetPeerFromPoint(point) ?? children[i];
			}
		return null;
	}
}
