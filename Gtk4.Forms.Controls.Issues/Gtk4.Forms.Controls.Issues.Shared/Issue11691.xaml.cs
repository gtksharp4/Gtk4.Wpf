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
	[Category(UITestCategories.Shape)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11691,
		"Polygon Shape doesn't work with DynamicResource color binding",
		PlatformAffected.All)]
	public partial class Issue11691 : TestContentPage
	{
		public Issue11691()
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