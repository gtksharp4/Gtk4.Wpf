using System;
using Gtk4.Forms;
using Gtk4.Forms.PlatformConfiguration;
using Gtk4.Forms.PlatformConfiguration.iOSSpecific;

namespace Gtk4.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
{
	public class EntryPageiOS : ContentPage
	{
		public EntryPageiOS()
		{
			var entry = new Entry
			{
				FontSize = 22,
				Placeholder = "Type and toggle AdjustsFontSizeToFitWidth"
			};

			Content = new StackLayout
			{
				Children =
				{
					entry,
					new Button
					{
						Text = "Toggle AdjustsFontSizeToFitWidth",
						Command = new Command(() => entry.On<iOS>().SetAdjustsFontSizeToFitWidth(!entry.On<iOS>().AdjustsFontSizeToFitWidth()))
					}
				}
			};
		}
	}
}
