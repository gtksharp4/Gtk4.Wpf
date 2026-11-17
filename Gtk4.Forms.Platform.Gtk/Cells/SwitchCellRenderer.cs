using System.ComponentModel;

namespace Gtk4.Forms.Platform.GTK.Cells;

public class SwitchCellRenderer : CellRenderer
{
	public override CellBase GetCell(Cell item, Gtk.Container reusableView, Controls.ListView listView)
	{
		var switchCell = base.GetCell(item, reusableView, listView) as SwitchCell;

		switchCell.Toggled -= OnToggled;
		switchCell.Toggled += OnToggled;

		return switchCell;
	}

	protected override Gtk.Container GetCellWidgetInstance(Cell item)
	{
		var switchCell = (Gtk4.Forms.SwitchCell)item;

		var text = switchCell.Text ?? string.Empty;
		var on = switchCell.On;

		return new SwitchCell(text, on);
	}

	protected override void CellPropertyChanged(object sender, PropertyChangedEventArgs args)
	{
		base.CellPropertyChanged(sender, args);

		var gtkSwitchCell = (SwitchCell)sender;
		var switchCell = (Gtk4.Forms.SwitchCell)gtkSwitchCell.Cell;

		if (args.PropertyName == Gtk4.Forms.SwitchCell.TextProperty.PropertyName)
		{
			gtkSwitchCell.Text = switchCell.Text ?? string.Empty;
		}
		else if (args.PropertyName == Gtk4.Forms.SwitchCell.OnProperty.PropertyName)
		{
			gtkSwitchCell.On = switchCell.On;
		}
	}

	private void OnToggled(object sender, bool active)
	{
		((Gtk4.Forms.SwitchCell)Cell).On = active;
	}
}