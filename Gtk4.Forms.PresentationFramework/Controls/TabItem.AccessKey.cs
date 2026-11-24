using System.Windows.Input;

namespace System.Windows.Controls;

public partial class TabItem
{
	Gtk.Label? _tabLabel;

	/// <summary>Builds the tab-strip label: a string header recognizes access keys, any other header is literal.</summary>
	internal Gtk.Label CreateTabLabel()
	{
		_tabLabel = new Gtk.Label(string.Empty);
		ApplyTabLabel();
		return _tabLabel;
	}

	void ApplyTabLabel()
	{
		if (Header is string text)
		{
			var mnemonic = AccessKeyBinding.Apply(this, text);
			if (_tabLabel != null) AccessKeyBinding.SetLabel(_tabLabel, mnemonic);
			return;
		}
		AccessKeyBinding.Register(this, null);
		if (_tabLabel != null) _tabLabel.Text = HeaderText;
	}

	/// <inheritdoc/>
	protected override void OnHeaderChanged(object? oldHeader, object? newHeader) => ApplyTabLabel();

	/// <summary>Selects the tab and focuses it.</summary>
	protected override void OnAccessKey(AccessKeyEventArgs e)
	{
		if (!IsSelected) IsSelected = true;
		Focus();
	}
}
