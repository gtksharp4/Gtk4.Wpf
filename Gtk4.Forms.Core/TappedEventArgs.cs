using System;

namespace Gtk4.Forms
{
	public class TappedEventArgs : EventArgs
	{
		public TappedEventArgs(object parameter)
		{
			Parameter = parameter;
		}

		public object Parameter { get; private set; }
	}
}