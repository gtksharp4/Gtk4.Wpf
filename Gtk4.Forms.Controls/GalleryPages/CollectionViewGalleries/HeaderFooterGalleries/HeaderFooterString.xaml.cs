using Gtk4.Forms.Xaml;

namespace Gtk4.Forms.Controls.GalleryPages.CollectionViewGalleries.HeaderFooterGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HeaderFooterString : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource(3);

		public HeaderFooterString()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();
			CollectionView.ItemsSource = _demoFilteredItemSource.Items;
		}
	}
}