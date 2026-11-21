using System.Collections.Generic;

namespace System.Windows.Automation.Peers;

public abstract partial class AutomationPeer
{
	protected abstract string GetClassNameCore();

	protected abstract string GetNameCore();

	protected abstract string GetAutomationIdCore();

	protected abstract string GetHelpTextCore();

	protected abstract AutomationControlType GetAutomationControlTypeCore();

	protected abstract Rect GetBoundingRectangleCore();

	protected abstract Point GetClickablePointCore();

	protected abstract List<AutomationPeer>? GetChildrenCore();

	protected abstract AutomationOrientation GetOrientationCore();

	protected abstract bool HasKeyboardFocusCore();

	protected abstract bool IsContentElementCore();

	protected abstract bool IsControlElementCore();

	protected abstract bool IsEnabledCore();

	protected abstract bool IsKeyboardFocusableCore();

	protected abstract bool IsOffscreenCore();

	protected abstract bool IsPasswordCore();

	protected abstract bool IsRequiredForFormCore();

	protected abstract void SetFocusCore();

	protected virtual string GetAcceleratorKeyCore() => string.Empty;

	protected virtual string GetAccessKeyCore() => string.Empty;

	protected virtual string GetItemStatusCore() => string.Empty;

	protected virtual string GetItemTypeCore() => string.Empty;

	protected virtual AutomationPeer? GetLabeledByCore() => null;

	protected virtual AutomationLiveSetting GetLiveSettingCore() => AutomationLiveSetting.Off;

	protected virtual int GetPositionInSetCore() => -1;

	protected virtual int GetSizeOfSetCore() => -1;

	protected virtual int GetLevelCore() => -1;

	protected virtual bool IsDialogCore() => false;

	protected virtual AutomationHeadingLevel GetHeadingLevelCore() => AutomationHeadingLevel.None;

	protected virtual List<AutomationPeer>? GetControlledPeersCore() => null;

	protected virtual string GetLocalizedControlTypeCore()
	{
		var type = ControlType.LookupById(50000 + (int)GetAutomationControlType());
		return type?.LocalizedControlType ?? string.Empty;
	}

	protected virtual AutomationPeer? GetPeerFromPointCore(Point point)
	{
		var children = GetChildren();
		if (children != null)
			for (int i = children.Count - 1; i >= 0; i--)
			{
				var bounds = children[i].GetBoundingRectangle();
				if (!bounds.IsEmpty && point.X >= bounds.X && point.X <= bounds.X + bounds.Width
					&& point.Y >= bounds.Y && point.Y <= bounds.Y + bounds.Height)
					return children[i].GetPeerFromPoint(point) ?? children[i];
			}
		return null;
	}
}
