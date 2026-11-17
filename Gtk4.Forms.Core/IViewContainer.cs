using System.Collections.Generic;

namespace Gtk4.Forms
{
	public interface IViewContainer<T> where T : VisualElement
	{
		IList<T> Children { get; }
	}
}