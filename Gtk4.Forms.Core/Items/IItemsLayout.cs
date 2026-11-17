using System.ComponentModel;

namespace Gtk4.Forms
{
	[TypeConverter(typeof(ItemsLayoutTypeConverter))]
	public interface IItemsLayout : INotifyPropertyChanged { }
}