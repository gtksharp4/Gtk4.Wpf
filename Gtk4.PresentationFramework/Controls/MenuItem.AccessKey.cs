using System.Windows.Input;

namespace System.Windows.Controls;

public partial class MenuItem
{
	// A string header recognizes access keys (WPF's MenuItem template); other headers are literal.
	string HeaderMnemonic()
	{
		if (Header is string text) return AccessKeyBinding.Apply(this, text);
		AccessKeyBinding.Register(this, null);
		return HeaderProjection(Header).Replace("_", "__");
	}

	// GTK popover menu items are text-only: an element header shows its text.
	static string HeaderProjection(object? header) => header switch
	{
		TextBlock tb => tb.Text ?? string.Empty,
		AccessText at => at.Text ?? string.Empty,
		_ => ContentResolution.ToText(header, null),
	};

	/// <summary>Focuses the item, then opens its submenu or clicks it unless other elements share the key.</summary>
	protected override void OnAccessKey(AccessKeyEventArgs e)
	{
		base.OnAccessKey(e);
		if (e.IsMultiple) return;
		if (HasSubItems)
		{
			IsSubmenuOpen = true;
			return;
		}
		Activate();
		for (var p = LogicalParent as MenuItem; p != null; p = p.LogicalParent as MenuItem)
			p.IsSubmenuOpen = false;
	}
}
