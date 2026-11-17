using System;

using Gtk4.Forms;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 27581, "WebView in ContentPage", PlatformAffected.Android)]
	public class Bugzilla27581 : ContentPage
	{
		public Bugzilla27581()
		{
			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = {
					new Label {
#pragma warning disable 618
						XAlign = TextAlignment.Center,
#pragma warning restore 618
						Text = "Tap input field, then try to scroll"
					},
					new WebView {
						Source = "http://movinglabs.com/temp/xamarin/input.html",
						VerticalOptions = LayoutOptions.FillAndExpand
					}
				}
			};
		}
	}
}
