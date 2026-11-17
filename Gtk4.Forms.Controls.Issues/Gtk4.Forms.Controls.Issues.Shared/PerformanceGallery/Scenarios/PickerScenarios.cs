using System;
using System.Linq;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class PickerScenario1 : PerformanceScenario
	{
		public PickerScenario1()
		: base("[Picker] Empty")
		{
			View = new Picker();
		}
	}
}
