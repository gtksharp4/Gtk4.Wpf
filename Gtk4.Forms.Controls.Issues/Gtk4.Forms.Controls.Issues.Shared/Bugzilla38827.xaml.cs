using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk4.Forms;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 38827, "UWP - Text not wrapping")]
	public partial class Bugzilla38827 : ContentPage
	{
		public Bugzilla38827()
		{
#if !UITEST
			InitializeComponent();
#endif
		}
	}
}
