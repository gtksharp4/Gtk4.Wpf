using System;
using System.Linq;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class DatePickerScenario1 : PerformanceScenario
	{
		public DatePickerScenario1()
		: base("[DatePicker] Empty")
		{
			View = new DatePicker();
		}
	}
}
