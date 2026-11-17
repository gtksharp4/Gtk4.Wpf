using System;

namespace Gtk4.Forms
{
	internal class ToolbarItemEventArgs : EventArgs
	{
		public ToolbarItemEventArgs(ToolbarItem item)
		{
			ToolbarItem = item;
		}

		public ToolbarItem ToolbarItem { get; private set; }
	}
}