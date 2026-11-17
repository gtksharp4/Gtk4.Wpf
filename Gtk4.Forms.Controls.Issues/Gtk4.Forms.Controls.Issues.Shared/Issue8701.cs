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
	[Issue(IssueTracker.Github, 8701, "TalkBack reads labeled back button as 'Unlabeled button'", PlatformAffected.Android)]
	public class Issue8701 : TestNavigationPage
	{
		protected override void Init()
		{
			Navigation.PushAsync(new Issue8701FirstPage());
		}

		class Issue8701FirstPage : ContentPage
		{
			public Issue8701FirstPage()
			{
				Title = "Issue 8701 first page";
				SetBackButtonTitle(this, "Back button title is set");

				Button button = new Button() { Text = "Navigate to the second page" };
				button.Clicked += async (sender, args) => await Navigation.PushAsync(new Issue8701SecondPage());

				Content = new StackLayout()
				{
					Margin = 20,

					Children =
					{
						button
					}
				};

			}
		}

		class Issue8701SecondPage : ContentPage
		{
			public Issue8701SecondPage()
			{
				Title = "Issue 8701 second page";

				Label label = new Label() { Text = "Enable TalkBack and tap on the back button. Test passes if TalkBack reads: 'Back button title is set'" };

				Content = new StackLayout()
				{
					Margin = 20,

					Children =
					{
						label
					}
				};
			}
		}
	}
}