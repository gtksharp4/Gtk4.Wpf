using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.RefreshViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class RefreshScrollViewGallery : ContentPage
	{
		public RefreshScrollViewGallery()
		{
			InitializeComponent();
			BindingContext = new RefreshViewModel(true);
		}
	}
}