using System;
using System.Threading.Tasks;
using Gtk4.Forms;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;
using Gtk4.Forms.PlatformConfiguration;
using Gtk4.Forms.PlatformConfiguration.iOSSpecific;
using NavigationPage = Gtk4.Forms.NavigationPage;
using Page = Gtk4.Forms.Page;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
#endif

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12300, "Crash when dismissing modally presented page on iOS", PlatformAffected.iOS)]
	public class Issue12300 : TestContentPage
	{
		protected override void Init()
		{
			var button = new Button()
			{
				Text = "Show Modal Page",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.CenterAndExpand,
			};

			button.Clicked += (s, a) =>
			{
				Navigation.PushModalAsync(new OTNavigationPage(new ModalPage()));
			};

			Content = new StackLayout()
			{
				Children =
				{
					button
				}
			};
		}

		public class OTNavigationPage : NavigationPage
		{
			public OTNavigationPage(Page page) : base(page)
			{
				On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.PageSheet);
			}
		}

		public class ModalPage : ContentPage
		{
			public ModalPage()
			{
				var button = new Button()
				{
					Text = "Hide Modal Page",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.CenterAndExpand,
				};

				button.Clicked += (s, a) =>
				{
					Navigation.PopModalAsync();
				};

				Content = new StackLayout()
				{
					Children =
					{
						button
					}
				};
			}

			private void ButtonClicked(object sender, EventArgs e)
			{
				Navigation.PopModalAsync();
			}
		}
	}
}
