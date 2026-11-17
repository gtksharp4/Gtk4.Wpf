using System;
using System.Linq;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class LabelScenario1 : PerformanceScenario
	{
		public LabelScenario1()
		: base("[Label] Empty")
		{
			View = new Label();
		}
	}
}
