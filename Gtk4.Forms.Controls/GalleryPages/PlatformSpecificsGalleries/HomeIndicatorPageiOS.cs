using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Gtk4.Forms.PlatformConfiguration;
using Gtk4.Forms.PlatformConfiguration.iOSSpecific;

namespace Gtk4.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
{
	public class HomeIndicatorPageiOS : ContentPage
	{
		public HomeIndicatorPageiOS(ICommand restore)
		{
			Title = "Large Titles";

			var offscreenPageLimit = new Label();
			var content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = LayoutOptions.Fill,
				Children =
				{
					new Button
					{
						Text = "Show Home indicator",
						Command = new Command(() => On<iOS>().SetPrefersHomeIndicatorAutoHidden(false))
					},
					new Button
					{
						Text = "Hide Home indicator",
						Command = new Command(() => On<iOS>().SetPrefersHomeIndicatorAutoHidden(true))
					},
					offscreenPageLimit
				}
			};

			var restoreButton = new Button { Text = "Back To Gallery" };
			restoreButton.Clicked += async (sender, args) => await Navigation.PopAsync();
			content.Children.Add(restoreButton);

			Content = content;
		}
	}
}
