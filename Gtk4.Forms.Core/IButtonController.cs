using System;
using System.Windows.Input;

namespace Gtk4.Forms
{
	public interface IButtonController : IViewController
	{
		void SendClicked();
		void SendPressed();
		void SendReleased();
	}
}