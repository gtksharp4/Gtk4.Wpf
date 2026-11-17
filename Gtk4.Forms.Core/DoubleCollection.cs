using System.Collections.ObjectModel;

namespace Gtk4.Forms
{
	[TypeConverter(typeof(DoubleCollectionConverter))]
	public sealed class DoubleCollection : ObservableCollection<double>
	{

	}
}