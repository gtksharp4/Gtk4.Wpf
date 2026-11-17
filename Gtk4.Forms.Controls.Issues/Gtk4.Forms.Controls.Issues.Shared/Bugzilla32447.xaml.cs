using System;
using System.Collections.Generic;

using Gtk4.Forms;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 32447, "[iOS] App crash when scrolling quickly through a TableView that has Pickers in the cells.", PlatformAffected.iOS)]
	public partial class Bugzilla32447 : TestContentPage
	{
		public Bugzilla32447()
		{
#if APP

			InitializeComponent();

#endif
		}

		protected override void Init()
		{

		}
	}
}

