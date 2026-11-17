using System;

namespace Gtk4.Forms.Platform.GTK.Controls;

public class NavigationChildPage : IDisposable
{
	bool _disposed;

	public NavigationChildPage(Gtk4.Forms.Page page)
	{
		Page = page;
		Identifier = Guid.NewGuid().ToString();
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			Page = null;
		}
	}

	public string Identifier { get; set; }

	public Gtk4.Forms.Page Page { get; private set; }
}