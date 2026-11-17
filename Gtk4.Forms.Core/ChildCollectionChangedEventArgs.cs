using System;
using System.Collections.Specialized;

namespace Gtk4.Forms
{
	internal class ChildCollectionChangedEventArgs : EventArgs
	{
		public ChildCollectionChangedEventArgs(NotifyCollectionChangedEventArgs args)
		{
			Args = args;
		}

		public NotifyCollectionChangedEventArgs Args { get; private set; }
	}
}