using System;

namespace Gtk4.Forms
{
	public class BackButtonPressedEventArgs : EventArgs
	{
		public bool Handled { get; set; }
	}
}