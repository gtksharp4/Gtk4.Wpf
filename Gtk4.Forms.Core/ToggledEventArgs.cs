using System;

namespace Gtk4.Forms
{
	public class ToggledEventArgs : EventArgs
	{
		public ToggledEventArgs(bool value)
		{
			Value = value;
		}

		public bool Value { get; private set; }
	}
}