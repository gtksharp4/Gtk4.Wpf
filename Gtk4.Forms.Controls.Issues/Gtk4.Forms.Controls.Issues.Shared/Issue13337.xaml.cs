using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
#endif

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 13337,
		"[Bug] [5.0] [Android] Interacting with a SwipeView on a TabbedPage with IsSwipePagingEnabled=false re-enables page swiping",
		PlatformAffected.Android)]
	public partial class Issue13337 : TestTabbedPage
	{
		public Issue13337()
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