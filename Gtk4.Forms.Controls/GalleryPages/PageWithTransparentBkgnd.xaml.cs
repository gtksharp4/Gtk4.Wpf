using System;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages
{
	[Preserve(AllMembers = true)]
	public partial class PageWithTransparentBkgnd : ContentPage
	{
		public PageWithTransparentBkgnd()
		{
			InitializeComponent();
		}

		void ClosePageButtonClicked(object sender, EventArgs e)
		{
			Navigation.PopModalAsync();
		}
	}
}