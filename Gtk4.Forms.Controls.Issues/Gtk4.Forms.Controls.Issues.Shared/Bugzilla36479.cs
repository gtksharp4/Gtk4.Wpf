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
	[Issue(IssueTracker.Bugzilla, 36479, "[WP8] Picker is not disabled when IsEnabled is set to false", PlatformAffected.WinPhone)]
	public class Bugzilla36479 : TestContentPage
	{
		protected override void Init()
		{
			var picker = new Picker
			{
				IsEnabled = false
			};
			picker.Items.Add("item");
			picker.Items.Add("item 2");

			Content = new StackLayout
			{
				Children =
				{
					picker,
					new Button
					{
						Command = new Command(() =>
						{
							if (picker.IsEnabled)
								picker.IsEnabled = false;
							else
								picker.IsEnabled = true;
						}),
						Text = "Enable/Disable Picker"
					}
				}
			};
		}
	}
}
