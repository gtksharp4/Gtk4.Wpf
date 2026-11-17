using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.Issues
{
#if APP
#if UITEST
		[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 54977, "Toolbaritems do not appear", PlatformAffected.Android, NavigationBehavior.PushAsync)]
	public partial class Bugzilla54977 : ContentPage
	{
		string _prefix;

		public Bugzilla54977()
		{
			InitializeComponent();
		}
	}
#endif
}
