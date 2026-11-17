using System;
namespace Gtk4.Forms
{
	public class CurrentItemChangedEventArgs : EventArgs
	{
		public object PreviousItem { get; }
		public object CurrentItem { get; }

		internal CurrentItemChangedEventArgs(object previousItem, object currentItem)
		{
			PreviousItem = previousItem;
			CurrentItem = currentItem;
		}
	}
}
