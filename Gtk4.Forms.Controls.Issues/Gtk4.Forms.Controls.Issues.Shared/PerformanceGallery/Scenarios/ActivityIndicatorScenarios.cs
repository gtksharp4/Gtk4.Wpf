using System;
using System.Linq;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class ActivityIndicatorScenario1 : PerformanceScenario
	{
		public ActivityIndicatorScenario1()
		: base("[ActivityIndicator] Running")
		{
			View = new ActivityIndicator { IsRunning = true };
		}
	}
}
