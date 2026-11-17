using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.RefreshViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class RefreshLayoutGallery : ContentPage
	{
		public RefreshLayoutGallery()
		{
			InitializeComponent();
			BindingContext = new RefreshViewModel();
		}
	}
}