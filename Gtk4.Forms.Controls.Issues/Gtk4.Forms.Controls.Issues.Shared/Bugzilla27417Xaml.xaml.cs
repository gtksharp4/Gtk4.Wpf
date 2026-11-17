using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk4.Forms;

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	public partial class Bugzilla27417Xaml : ContentPage
	{
		public Bugzilla27417Xaml()
		{
#if APP
			InitializeComponent();
#endif
		}
	}
}
