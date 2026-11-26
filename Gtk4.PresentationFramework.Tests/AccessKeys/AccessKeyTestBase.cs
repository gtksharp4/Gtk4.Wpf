using System;
using System.Collections.Generic;
using System.Windows;
using Gtk4.PresentationFramework.Tests.Lifecycle;

namespace Gtk4.PresentationFramework.Tests.AccessKeys;

/// <summary>
/// Shows windows for access-key tests and closes them all afterwards, so keys never leak between tests.
/// </summary>
public abstract class AccessKeyTestBase : WpfGtkTestBase, IDisposable
{
	readonly List<Window> _windows = new();

	protected Window Show(UIElement content)
	{
		var window = new Window { Content = content, Width = 400, Height = 300 };
		_windows.Add(window);
		window.Show();
		GtkPump.Until(() => window.IsVisible && content.IsVisible);
		return window;
	}

	public void Dispose()
	{
		if (_windows.Count == 0) return;
		Run(() =>
		{
			foreach (var window in _windows)
				window.Close();
			GtkPump.For(50);
		});
	}
}
