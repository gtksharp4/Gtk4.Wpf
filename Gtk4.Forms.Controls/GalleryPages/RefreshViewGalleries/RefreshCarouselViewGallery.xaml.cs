using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.RefreshViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class RefreshCarouselViewGallery : ContentPage
	{
		public RefreshCarouselViewGallery()
		{
			InitializeComponent();
			BindingContext = new RefreshViewModel();
		}
	}
}