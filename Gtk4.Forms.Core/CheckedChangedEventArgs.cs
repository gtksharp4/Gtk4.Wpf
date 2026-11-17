using System;

namespace Gtk4.Forms
{
	public class CheckedChangedEventArgs : EventArgs
	{
		public CheckedChangedEventArgs(bool value)
		{
			Value = value;
		}

		public bool Value { get; private set; }
	}
}