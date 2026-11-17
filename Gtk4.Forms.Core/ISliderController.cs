using System.ComponentModel;

namespace Gtk4.Forms
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ISliderController
	{
		void SendDragStarted();
		void SendDragCompleted();
	}
}
