using System;
using System.Linq;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class WebViewScenario1 : PerformanceScenario
	{
		public WebViewScenario1()
		: base("[WebView] Empty")
		{
			View = new WebView();
		}
	}
}
