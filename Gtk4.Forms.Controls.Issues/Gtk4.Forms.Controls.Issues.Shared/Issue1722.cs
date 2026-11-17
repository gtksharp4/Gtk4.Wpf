using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1722, "FlyoutPage crashes when assigning a NavigationPage to Detail with no children pushed", PlatformAffected.iOS)]
	public class Issue1722 : FlyoutPage
	{
		public Issue1722()
		{
			Flyout = new ContentPage
			{
				Title = "Flyout",
				Content = new Label { Text = "Flyout" }
			};

			Detail = new NavigationPage();
		}
	}
}
