using System;
using System.Linq;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class TimePickerScenario1 : PerformanceScenario
	{
		public TimePickerScenario1()
		: base("[TimePicker] Empty")
		{
			View = new TimePicker();
		}
	}
}
