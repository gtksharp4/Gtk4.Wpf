using System.Windows.Input;

namespace System.Windows.Controls;

public partial class CheckBox
{
	/// <summary>Takes focus, then focuses or toggles as <see cref="Primitives.ButtonBase"/> does.</summary>
	protected override void OnAccessKey(AccessKeyEventArgs e)
	{
		if (!IsKeyboardFocused) Focus();
		base.OnAccessKey(e);
	}
}
