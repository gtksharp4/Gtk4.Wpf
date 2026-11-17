using System;

namespace Gtk4.Forms
{
	public interface IAppLinks
	{
		void DeregisterLink(IAppLinkEntry appLink);
		void DeregisterLink(Uri appLinkUri);
		void RegisterLink(IAppLinkEntry appLink);
	}
}