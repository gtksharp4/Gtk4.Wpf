using Gtk4.Forms.Xaml;

namespace Gtk4.Forms.Controls.GalleryPages.CollectionViewGalleries.AlternateLayoutGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class StaggeredLayout : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource();

		public StaggeredLayout()
		{
			InitializeComponent();

			CV.ItemTemplate = ExampleTemplates.RandomSizeTemplate();
			CV.ItemsSource = _demoFilteredItemSource.Items;
		}
	}

	public class StaggeredCollectionView : CollectionView { }

	public class StaggeredGridItemsLayout : GridItemsLayout
	{
		public StaggeredGridItemsLayout([Parameter("Orientation")] ItemsLayoutOrientation orientation) : base(orientation)
		{
		}

		public StaggeredGridItemsLayout(int span, [Parameter("Orientation")] ItemsLayoutOrientation orientation) : base(span, orientation)
		{
		}
	}
}