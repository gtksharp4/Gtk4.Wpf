using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 29107, "Xamarin.Android ScrollView text overlaps", PlatformAffected.Android)]
	public partial class Bugzilla29107 : TestContentPage
	{
		public Bugzilla29107()
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
