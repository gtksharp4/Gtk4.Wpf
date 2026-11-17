using System.Collections.Generic;

namespace Gtk4.Forms
{
	public interface IGestureRecognizers
	{
		IList<IGestureRecognizer> GestureRecognizers { get; }
	}
}