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
	[Category(UITestCategories.Frame)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11120, "[Bug] IsClippedToBounds iOS not work", PlatformAffected.iOS)]
	public partial class Issue11120 : ContentPage
	{
		public Issue11120()
		{
#if APP
			InitializeComponent();
#endif

		}
	}
}