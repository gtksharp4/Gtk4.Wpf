using System.Windows.Input;

namespace System.Windows.Controls.Primitives;

public abstract partial class ButtonBase
{
	/// <summary>Clicks the button, or only focuses it when other elements share the key.</summary>
	protected override void OnAccessKey(AccessKeyEventArgs e)
	{
		if (e.IsMultiple)
			base.OnAccessKey(e);
		else
			OnClick();
	}

	/// <summary>Text for the GTK label of this button's string content, registering its access key.</summary>
	internal string AccessLabel(object? content) => AccessKeyBinding.Apply(this, content as string);
}
