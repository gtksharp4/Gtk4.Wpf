using System;

namespace Gtk4.Forms
{
	public class DropCompletedEventArgs : EventArgs
	{
		DataPackageOperation DropResult { get; }
	}
}
