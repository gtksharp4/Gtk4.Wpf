using System;
using System.Collections.Generic;

namespace Gtk4.Forms
{
	public interface ISearchHandlerController
	{
		event EventHandler<ListProxyChangedEventArgs> ListProxyChanged;

		IReadOnlyList<object> ListProxy { get; }

		void ClearPlaceholderClicked();

		void ItemSelected(object obj);

		void QueryConfirmed();
	}
}