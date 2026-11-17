using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.RefreshViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class RefreshCollectionViewGallery : ContentPage
	{
		public RefreshCollectionViewGallery()
		{
			InitializeComponent();
			BindingContext = new RefreshViewModel();
		}
	}
}