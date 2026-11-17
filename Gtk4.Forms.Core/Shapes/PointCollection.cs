using System.Collections.ObjectModel;

namespace Gtk4.Forms.Shapes
{
	[TypeConverter(typeof(PointCollectionConverter))]
	public sealed class PointCollection : ObservableCollection<Point>
	{

	}
}