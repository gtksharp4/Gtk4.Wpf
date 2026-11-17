using System;
using System.Linq;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class TableViewScenario1 : PerformanceScenario
	{
		public TableViewScenario1()
		: base("[TableView] Empty")
		{
			View = new TableView();
		}
	}
}
