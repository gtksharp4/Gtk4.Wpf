using System;
using System.Linq;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class SwitchScenario1 : PerformanceScenario
	{
		public SwitchScenario1()
		: base("[Switch] Empty")
		{
			View = new Switch();
		}
	}
}
