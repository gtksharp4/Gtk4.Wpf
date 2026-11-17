using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 55714, "[UWP] Cannot set Editor text color", PlatformAffected.UWP)]
	public class Bugzilla55714 : TestContentPage
	{
		protected override void Init()
		{
			var editor = new Editor
			{
				TextColor = Color.Yellow,
				BackgroundColor = Color.Black
			};

			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "The below Editor should have visible yellow text when entered"
					},
					editor,
					new Button
					{
						Text = "Change Editor text color to white",
						Command = new Command(() => editor.TextColor = Color.White)
					}
				}
			};
		}
	}
}