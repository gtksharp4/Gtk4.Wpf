using System;
using System.Linq;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class ProgressBarScenario1 : PerformanceScenario
	{
		public ProgressBarScenario1()
		: base("[ProgressBar] Empty")
		{
			View = new ProgressBar();
		}
	}
}
