using System;

namespace Gtk4.Forms
{
	public interface IOpenGlViewController : IViewController
	{
		event EventHandler DisplayRequested;
	}
}