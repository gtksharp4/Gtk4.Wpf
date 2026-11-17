using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public class SwipeCollectionViewGallery : ContentPage
	{
		public SwipeCollectionViewGallery()
		{
			Title = "CollectionView Galleries";
			Content = new StackLayout
			{
				Children =
				{
					GalleryBuilder.NavButton("Horizontal CollectionView Gallery", () => new SwipeHorizontalCollectionViewGallery(), Navigation),
					GalleryBuilder.NavButton("Vertical CollectionView Gallery", () => new SwipeVerticalCollectionViewGallery(), Navigation)
				}
			};
		}
	}
}