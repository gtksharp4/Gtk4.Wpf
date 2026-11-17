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
	[Issue(IssueTracker.Github, 11081, "[Bug] CarouselView should not animate an initial Position on Android",
		PlatformAffected.Android)]
#if UITEST
	[Category(UITestCategories.CarouselView)]
#endif
	public sealed partial class Issue11081 : TestContentPage
	{
		public Issue11081()
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