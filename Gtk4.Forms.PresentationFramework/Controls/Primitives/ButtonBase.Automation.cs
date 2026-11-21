namespace System.Windows.Controls.Primitives;

public abstract partial class ButtonBase
{
	// Clicks as a user would: raises Click (ToggleButton toggles first), then runs the command.
	internal void AutomationClick() => OnClick();
}