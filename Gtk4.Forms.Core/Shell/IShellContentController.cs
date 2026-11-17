using System;

namespace Gtk4.Forms
{
	public interface IShellContentController : IElementController
	{
		Page GetOrCreateContent();

		void RecyclePage(Page page);

		Page Page { get; }
		event EventHandler IsPageVisibleChanged;
	}
}