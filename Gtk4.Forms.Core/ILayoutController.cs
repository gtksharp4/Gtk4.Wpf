using System.Collections.Generic;

namespace Gtk4.Forms
{
	public interface ILayoutController
	{
		IReadOnlyList<Element> Children { get; }
	}
}