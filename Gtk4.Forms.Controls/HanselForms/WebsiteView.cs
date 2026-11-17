using System;

using Gtk4.Forms;

namespace Gtk4.Forms.Controls
{
	public class WebsiteView : BaseView
	{
		public WebsiteView(string site, string title)
		{
			this.Title = title;
			var webView = new WebView();
			webView.Source = new UrlWebViewSource
			{
				Url = site
			};
			Content = webView;
		}
	}
}

