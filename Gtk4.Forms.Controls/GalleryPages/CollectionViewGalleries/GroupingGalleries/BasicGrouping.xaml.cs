using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk4.Forms;
using Gtk4.Forms.Internals;
using Gtk4.Forms.Xaml;

namespace Gtk4.Forms.Controls.GalleryPages.CollectionViewGalleries.GroupingGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Preserve(AllMembers = true)]
	public partial class BasicGrouping : ContentPage
	{
		public BasicGrouping()
		{
			InitializeComponent();

			CollectionView.ItemsSource = new SuperTeams();
		}
	}
}