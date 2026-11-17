using System.Collections.Generic;

namespace Gtk4.Forms.Xaml
{
	internal interface IProvideParentValues : IProvideValueTarget
	{
		IEnumerable<object> ParentObjects { get; }
	}
}