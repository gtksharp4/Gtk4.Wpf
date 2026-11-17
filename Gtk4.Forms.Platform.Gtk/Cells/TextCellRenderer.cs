using System.ComponentModel;
using Gtk4.Forms.Internals;
using Gtk4.Forms.Platform.GTK.Extensions;

namespace Gtk4.Forms.Platform.GTK.Cells;

public class TextCellRenderer : CellRenderer
{
	public override CellBase GetCell(Cell item, Gtk.Container reusableView, Controls.ListView listView)
	{
		var gtkTextCell = base.GetCell(item, reusableView, listView) as TextCell;
		var textCell = (Gtk4.Forms.TextCell)item;

		gtkTextCell.IsGroupHeader = textCell.GetIsGroupHeader<ItemsView<Cell>, Cell>();

		return gtkTextCell;
	}

	protected override Gtk.Container GetCellWidgetInstance(Cell item)
	{
		var textCell = (Gtk4.Forms.TextCell)item;

		var text = textCell.Text ?? string.Empty;
		var textColor = textCell.TextColor.ToGtkColor();
		var detail = textCell.Detail ?? string.Empty;
		var detailColor = textCell.DetailColor.ToGtkColor();

		return new TextCell(
			text,
			textColor,
			detail,
			detailColor);
	}

	protected override void CellPropertyChanged(object sender, PropertyChangedEventArgs args)
	{
		base.CellPropertyChanged(sender, args);

		var gtkTextCell = (TextCell)sender;
		var textCell = (Gtk4.Forms.TextCell)gtkTextCell.Cell;

		if (args.PropertyName == Gtk4.Forms.TextCell.TextProperty.PropertyName)
		{
			gtkTextCell.Text = textCell.Text ?? string.Empty;
		}
		else if (args.PropertyName == Gtk4.Forms.TextCell.DetailProperty.PropertyName)
		{
			gtkTextCell.Detail = textCell.Detail ?? string.Empty;
		}
		else if (args.PropertyName == Gtk4.Forms.TextCell.TextColorProperty.PropertyName)
			gtkTextCell.TextColor = textCell.TextColor.ToGtkColor();
		else if (args.PropertyName == Gtk4.Forms.TextCell.DetailColorProperty.PropertyName)
			gtkTextCell.DetailColor = textCell.DetailColor.ToGtkColor();
	}
}