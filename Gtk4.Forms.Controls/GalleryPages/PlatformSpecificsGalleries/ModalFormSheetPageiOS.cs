using Gtk4.Forms.PlatformConfiguration;
using Gtk4.Forms.PlatformConfiguration.iOSSpecific;

namespace Gtk4.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
{
	public class ModalFormSheetPageiOS : ContentPage
	{
		public ModalFormSheetPageiOS()
		{
			Title = "Modal FormSheet";
			BackgroundColor = Color.Azure;

			On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);

			var restoreButton = new Button { Text = "Back To Gallery" };
			restoreButton.Clicked += (sender, args) => Navigation.PopModalAsync();

			Content = new StackLayout
			{
				Children =
				{
					restoreButton
				}
			};
		}
	}
}