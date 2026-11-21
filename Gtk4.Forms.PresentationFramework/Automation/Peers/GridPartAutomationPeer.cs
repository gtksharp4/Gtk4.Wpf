using System.Collections.Generic;

namespace System.Windows.Automation.Peers;

/// <summary>
/// Base of the peers for things inside a grid that are not elements of their own: rows, cells and headers.
/// They report the owning grid peer as their parent and take their state from it.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract class GridPartAutomationPeer : AutomationPeer
{
	internal GridPartAutomationPeer(FrameworkElementAutomationPeer grid)
	{
		GridPeer = grid;
	}

	internal FrameworkElementAutomationPeer GridPeer { get; }

	internal override AutomationPeer? ResolveParent() => GridPeer;

	internal virtual Gtk.Widget? PartWidget => null;

	protected override string GetHelpTextCore() => string.Empty;

	protected override string GetAutomationIdCore() => string.Empty;

	protected override Rect GetBoundingRectangleCore() => PeerGeometry.Bounds(PartWidget);

	protected override Point GetClickablePointCore()
	{
		var r = GetBoundingRectangle();
		return r.IsEmpty ? new Point(double.NaN, double.NaN) : new Point(r.X + r.Width / 2, r.Y + r.Height / 2);
	}

	protected override List<AutomationPeer>? GetChildrenCore() => null;

	protected override AutomationOrientation GetOrientationCore() => AutomationOrientation.None;

	protected override bool HasKeyboardFocusCore() => false;

	protected override bool IsContentElementCore() => true;

	protected override bool IsControlElementCore() => true;

	protected override bool IsEnabledCore() => GridPeer.IsEnabled();

	protected override bool IsKeyboardFocusableCore() => false;

	protected override bool IsOffscreenCore() => GetBoundingRectangle().IsEmpty;

	protected override bool IsPasswordCore() => false;

	protected override bool IsRequiredForFormCore() => false;

	protected override void SetFocusCore() => GridPeer.SetFocus();
}
