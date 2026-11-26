using System.Windows.Input;

namespace System.Windows.Controls;

public partial class GroupBox
{
	/// <summary>Moves focus to the first focusable element inside the group.</summary>
	protected override void OnAccessKey(AccessKeyEventArgs e)
		=> MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
}
