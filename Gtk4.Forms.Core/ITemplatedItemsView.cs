using System.ComponentModel;

namespace Gtk4.Forms
{
	public interface ITemplatedItemsView<TItem> : IItemsView<TItem> where TItem : BindableObject
	{
		event PropertyChangedEventHandler PropertyChanged;

		IListProxy ListProxy { get; }
		ITemplatedItemsList<TItem> TemplatedItems { get; }
	}
}
