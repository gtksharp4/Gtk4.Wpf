using System;

using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 31806, "[8.1/UWP] PopToRootAsync crashes due to not setting the current page correctly", PlatformAffected.WinRT)]
	public class Bugzilla31806 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				Children =
				{
					new Button
					{
						Text = "Navigate to a new page",
						Command = new Command(() =>
						{
							Navigation.PushAsync(new ContentPage
							{
								Content = new StackLayout
								{
									Children =
									{
										new Label
										{
											Text = "Pressing this button should let the navigation return to the repro list"
										},
										new Button
										{
											Text = "Call PopToRootAsync()",
											Command = new Command(() =>
											{
												Navigation.PopToRootAsync();
											})
										}
									}
								}
							});
						})
					}
				}
			};
		}
	}
}
