using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gtk4.Forms
{
	public interface ITemplatedItemsListScrollToRequestedEventArgs
	{
		object Group { get; }
		object Item { get; }
	}
}
