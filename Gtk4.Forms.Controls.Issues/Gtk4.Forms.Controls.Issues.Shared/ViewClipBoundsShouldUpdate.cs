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
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 888888, "Bounds clipping does not update when View bounds change", PlatformAffected.Android)]
	public class ViewClipBoundsShouldUpdate : TestContentPage
	{
		const string Success = "Success";

		class TestContentView : ContentView
		{
			public TestContentView()
			{
				Content = new Label { Text = Success };

				IsClippedToBounds = true;
			}
		}

		protected override void Init()
		{
			var layout = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = $"If '{Success}' displays below then this test has passed."
					},
					new TestContentView()
				}
			};

			Content = layout;
		}

	}
}