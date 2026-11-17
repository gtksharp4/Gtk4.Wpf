using Gtk4.Forms.Xaml;

namespace Gtk4.Forms.Controls.GalleryPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class IndicatorsSample : ContentPage
	{
		public IndicatorsSample()
		{
			InitializeComponent();
			BindingContext = new GalleryPages.CollectionViewGalleries.CarouselViewGalleries.CarouselItemsGalleryViewModel(false, false);
		}
	}
}