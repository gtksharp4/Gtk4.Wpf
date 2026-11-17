using System;
using System.Collections.Generic;

using Gtk4.Forms;

namespace Gtk4.Forms.Controls.GalleryPages
{
	public partial class IndicatorsTemplateSample : ContentPage
	{
		public IndicatorsTemplateSample()
		{
			InitializeComponent();
			BindingContext = new GalleryPages.CollectionViewGalleries.CarouselViewGalleries.CarouselItemsGalleryViewModel(false, false);

		}
	}
}
