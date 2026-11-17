using System;

namespace Gtk4.Forms
{
	public interface ITableViewController
	{
		event EventHandler ModelChanged;

		ITableModel Model { get; }
	}
}
