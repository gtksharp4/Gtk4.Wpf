using System.Collections.Generic;

namespace Gtk4.Forms.Internals
{
	public interface IGestureController
	{
		IList<GestureElement> GetChildElements(Point point);

		IList<IGestureRecognizer> CompositeGestureRecognizers { get; }
	}
}