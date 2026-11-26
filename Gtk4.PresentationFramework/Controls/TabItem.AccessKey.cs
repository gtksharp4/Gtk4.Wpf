using System.Windows.Input;

namespace System.Windows.Controls;

public partial class TabItem
{
	Gtk.Label? _tabLabel;

	/// <summary>
	/// Builds the tab-strip label: a string header recognizes access keys, any other header is literal.
	/// </summary>
	internal Gtk.Label CreateTabLabel()
	{
		_tabLabel = new Gtk.Label(string.Empty);
		ApplyTabLabel();
		return _tabLabel;
	}

	Gtk.Box? _tabHeaderBox;
	Gtk.Widget? _tabShown;

	/// <summary>The notebook tab widget: the text label, or the hosted header element.</summary>
	internal Gtk.Widget CreateTabWidget()
	{
		_tabHeaderBox = new Gtk.Box(Gtk.Orientation.Horizontal, 0);
		ApplyTabLabel();
		return _tabHeaderBox;
	}

	void ApplyTabLabel()
	{
		var headerWidget = ResolveHeaderWidget();
		if (_tabHeaderBox != null)
		{
			var wanted = headerWidget ?? (_tabLabel ??= new Gtk.Label(string.Empty));
			if (_tabShown != null && !ReferenceEquals(_tabShown, wanted)) _tabHeaderBox.Remove(_tabShown);
			if (!ReferenceEquals(_tabShown, wanted)) _tabHeaderBox.Add(wanted);
			_tabShown = wanted;
		}
		if (headerWidget != null)
		{
			AccessKeyBinding.Register(this, null);
			return;
		}
		if (HeaderIsAccessText)
		{
			var mnemonic = AccessKeyBinding.Apply(this, (string)Header!);
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
