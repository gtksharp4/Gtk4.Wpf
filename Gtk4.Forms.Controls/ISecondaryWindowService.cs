using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gtk4.Forms.Controls
{
	public interface ISecondaryWindowService
	{
		Task OpenSecondaryWindow(Type pageType);
	}
}
