using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
#endif

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Button)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 14428,
		"[Bug] [UWP] No visual distinguish between enabled & disabled buttons in XF ",
		PlatformAffected.UWP)]
	public partial class Issue14428 : TestContentPage
	{
		public Issue14428()
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