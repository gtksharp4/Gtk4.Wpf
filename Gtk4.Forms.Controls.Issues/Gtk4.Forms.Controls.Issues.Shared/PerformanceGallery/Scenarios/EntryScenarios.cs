using System;
using System.Linq;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class EntryScenario1 : PerformanceScenario
	{
		public EntryScenario1()
		: base("[Entry] Empty")
		{
			View = new Entry();
		}
	}
}
