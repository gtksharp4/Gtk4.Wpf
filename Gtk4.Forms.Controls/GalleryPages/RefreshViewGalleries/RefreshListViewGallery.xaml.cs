using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.RefreshViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class RefreshListViewGallery : ContentPage
	{
		public RefreshListViewGallery()
		{
			InitializeComponent();
			BindingContext = new RefreshViewModel();
		}
	}
}
