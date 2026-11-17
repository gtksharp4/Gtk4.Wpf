using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

#if UITEST
using Gtk4.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Brush)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 13378, "[Bug] iOS LinearGradientBrush Transparent Color",
		PlatformAffected.iOS)]
	public partial class Issue13378 : TestContentPage
	{
		public Issue13378()
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