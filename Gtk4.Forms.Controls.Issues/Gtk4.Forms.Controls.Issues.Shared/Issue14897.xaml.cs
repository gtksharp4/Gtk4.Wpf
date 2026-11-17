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
	[Issue(IssueTracker.Github, 14897, "[Bug] [5.0.0.2244] [Android] Interacting with a SwipeView inside a ScrollView on a TabbedPage with IsSwipePagingEnabled=false re-enables page swiping",
		PlatformAffected.Android)]
#if UITEST
	[Category(UITestCategories.SwipeView)]
#endif
	public sealed partial class Issue14897 : TestTabbedPage
	{
		public Issue14897()
		{
#if APP
			this.InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
	}
}