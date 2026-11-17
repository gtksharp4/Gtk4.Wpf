using System;
using System.Linq;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class SearchBarScenario1 : PerformanceScenario
	{
		public SearchBarScenario1()
		: base("[SearchBar] Empty")
		{
			View = new SearchBar();
		}
	}
}
