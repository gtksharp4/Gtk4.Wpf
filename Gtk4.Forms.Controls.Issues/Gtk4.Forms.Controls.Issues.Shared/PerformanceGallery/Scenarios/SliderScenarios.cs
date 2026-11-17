using System;
using System.Linq;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class SliderScenario1 : PerformanceScenario
	{
		public SliderScenario1()
		: base("[Slider] Empty")
		{
			View = new Slider();
		}
	}
}
