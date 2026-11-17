using System;
using System.Linq;
using Gtk4.Forms.Internals;
using Gtk4.Forms.Maps;

namespace Gtk4.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class MapScenario1 : PerformanceScenario
	{
		public MapScenario1()
		: base("[Map] Empty")
		{
			View = new Map();
		}
	}
}
