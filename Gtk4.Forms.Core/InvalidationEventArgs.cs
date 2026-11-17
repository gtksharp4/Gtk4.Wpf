using System;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms
{
	internal class InvalidationEventArgs : EventArgs
	{
		public InvalidationEventArgs(InvalidationTrigger trigger)
		{
			Trigger = trigger;
		}

		public InvalidationTrigger Trigger { get; private set; }
	}
}