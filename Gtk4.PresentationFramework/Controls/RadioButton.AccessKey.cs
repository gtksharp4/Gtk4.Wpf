using System.Windows.Input;

namespace System.Windows.Controls;

public partial class RadioButton
{
	/// <summary>Takes focus, then focuses or checks as <see cref="Primitives.ButtonBase"/> does.</summary>
	protected override void OnAccessKey(AccessKeyEventArgs e)
	{
		if (!IsKeyboardFocused) Focus();
		base.OnAccessKey(e);
	}
}
