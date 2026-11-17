using System;

namespace Gtk4.Forms
{
	public class DragStartingEventArgs : EventArgs
	{
		public DragStartingEventArgs()
		{
			Data = new DataPackage();
		}

		public bool Handled { get; set; }
		public bool Cancel { get; set; }
		public DataPackage Data { get; private set; }
	}
}
