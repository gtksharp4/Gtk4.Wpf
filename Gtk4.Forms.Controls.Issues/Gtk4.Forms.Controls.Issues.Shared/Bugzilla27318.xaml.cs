using System;
using System.Collections.Generic;
using Gtk4.Forms;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.Issues
{
#if APP
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 27318, "Labels overlapping", PlatformAffected.Android, NavigationBehavior.PushAsync)]
	public partial class Bugzilla27318 : ContentPage
	{
		public Bugzilla27318()
		{
			InitializeComponent();
			listView.ItemsSource = new[] { "Foo", "Bar", "Baz" };
		}
	}
#endif
}
