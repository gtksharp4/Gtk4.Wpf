using System.Windows.Input;

namespace System.Windows.Controls;

public partial class Expander
{
	// WPF's header is a ToggleButton inside the template that owns the key; here the Expander stands in for it.
	/// <summary>Toggles the expander, or only focuses it when other elements share the key.</summary>
	protected override void OnAccessKey(AccessKeyEventArgs e)
	{
		if (e.IsMultiple)
			base.OnAccessKey(e);
		else
			IsExpanded = !IsExpanded;
	}
}
