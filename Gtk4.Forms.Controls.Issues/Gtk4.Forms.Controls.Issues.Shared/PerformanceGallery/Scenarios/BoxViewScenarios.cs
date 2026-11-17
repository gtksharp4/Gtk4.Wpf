using System;
using System.Linq;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class BoxViewScenario1 : PerformanceScenario
	{
		public BoxViewScenario1()
		: base("[BoxView] Color set in ctor")
		{
			View = new BoxView { Color = Color.Red };
		}
	}
}
