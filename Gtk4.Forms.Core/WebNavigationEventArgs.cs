using System;

namespace Gtk4.Forms
{
	public class WebNavigationEventArgs : EventArgs
	{
		protected WebNavigationEventArgs(WebNavigationEvent navigationEvent, WebViewSource source, string url)
		{
			NavigationEvent = navigationEvent;
			Source = source;
			Url = url;
		}

		public WebNavigationEvent NavigationEvent { get; internal set; }

		public WebViewSource Source { get; internal set; }

		public string Url { get; internal set; }
	}
}