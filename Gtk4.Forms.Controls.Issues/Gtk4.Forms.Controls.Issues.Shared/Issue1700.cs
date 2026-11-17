using System;
using Gtk4.Forms;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
using System.Threading.Tasks;
#endif

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1700, "Image fails loading from long URL", PlatformAffected.iOS | PlatformAffected.Android | PlatformAffected.WinPhone)]
	public class Issue1700 : TestContentPage
	{
		const string Success = "Success";

		protected override void Init()
		{
			var stack = new StackLayout();
			var url = "https://github.com/pieroviano/Gtk4.Forms/raw/main/Gtk4.Forms.ControlGallery.Android/Resources/drawable/Legumes.jpg?a=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
			var url2 = "https://github.com/pieroviano/Gtk4.Forms/raw/main/Gtk4.Forms.ControlGallery.Android/Resources/drawable/Vegetables.jpg?a=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbasdasdasdasdasasdasdasdasdasd";
			var img = new Image
			{
				Source = new UriImageSource { Uri = new Uri(url) }
			};
			stack.Children.Add(img);
			var img2 = new Image
			{
				Source = new UriImageSource { Uri = new Uri(url2) }
			};
			stack.Children.Add(img2);

			var success = new Label { Text = Success };
			stack.Children.Add(success);

			Content = new ScrollView() { Content = stack };
		}

#if UITEST
		[Test]
		[Category(UITestCategories.Image)]
		public void LongImageURLsShouldNotCrash()
		{
			// Give the images some time to load (or fail)
			Task.Delay(3000).Wait();

			// If we can see this label at all, it means we didn't crash and the test is successful
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}


