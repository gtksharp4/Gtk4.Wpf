using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk4.Forms;
using Gtk4.Forms.Xaml;

namespace Gtk4.Forms.Controls.TabIndexTest
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DaysOfWeekView : Grid
	{
		public DaysOfWeekView()
		{
			InitializeComponent();
		}
	}
}