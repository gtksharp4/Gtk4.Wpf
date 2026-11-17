using System;

namespace Gtk4.Forms
{
	public sealed class ItemVisibilityEventArgs : EventArgs
	{
		[Obsolete("Please use the constructor that reports the items index")]
		public ItemVisibilityEventArgs(object item)
			: this(item, -1)
		{

		}

		public ItemVisibilityEventArgs(object item, int itemIndex)
		{
			Item = item;
			ItemIndex = itemIndex;
		}

		public object Item { get; private set; }

		public int ItemIndex { get; private set; }
	}
}