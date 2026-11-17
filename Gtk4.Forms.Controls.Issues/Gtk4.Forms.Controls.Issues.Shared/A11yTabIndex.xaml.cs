using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk4.Forms;
using Gtk4.Forms.Xaml;

namespace Gtk4.Forms.Controls.Issues
{
	public partial class A11yTabIndex : ContentPage
	{
		public A11yTabIndex()
		{
#if APP
			InitializeComponent();
#endif
		}
	}
}