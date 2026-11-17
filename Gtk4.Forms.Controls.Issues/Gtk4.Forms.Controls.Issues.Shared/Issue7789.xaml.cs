using System.Collections.Generic;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;
using Gtk4.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
#endif

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.CollectionView)]
	[Category(UITestCategories.UwpIgnore)] // CollectionView contents aren't currently visible to test automation
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7789, "CarouselView crash when displaying strings", PlatformAffected.iOS)]
	public partial class Issue7789 : TestContentPage
	{
		public Issue7789()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}

#if UITEST
		[Test]
		public void CarouselShouldNotCrashDisplayingStrings()
		{
			RunningApp.WaitForElement("Success");
		}
#endif
	}
}