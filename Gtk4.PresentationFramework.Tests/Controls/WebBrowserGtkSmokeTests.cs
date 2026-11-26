using System;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// GTK smoke tests for <see cref="WebBrowser"/>. Each body runs on the GTK thread.
/// Skipped when GTK is unavailable.  A <see cref="FakeWebBrowserHost"/> is injected so
/// no Photino native window is created even when a display is present.
/// </summary>
public class WebBrowserGtkSmokeTests : WpfGtkTestBase, IDisposable
{
	readonly Func<IWebBrowserHost> _originalFactory;

	public WebBrowserGtkSmokeTests()
	{
		_originalFactory = WebBrowser.HostFactory;
		// Always use fake host — never open a real Photino window in tests.
		WebBrowser.HostFactory = () => new FakeWebBrowserHost();
	}

	public void Dispose()
	{
		WebBrowser.HostFactory = _originalFactory;
	}

	[Fact]
	public void WebBrowser_CreateWidget_returns_GtkBox() => Run(() =>
	{
		using var wb = new WebBrowser();
		Assert.IsType<Gtk.Box>(wb.Widget);
	});

	[Fact]
	public void WebBrowser_CreateWidget_contains_placeholder_label() => Run(() =>
	{
		using var wb = new WebBrowser();
		var box = (Gtk.Box)wb.Widget;
		Assert.NotNull(box.FirstChild);
		Assert.IsType<Gtk.Label>(box.FirstChild);
	});

	[Fact]
	public void WebBrowser_Navigate_does_not_create_Photino_window() => Run(() =>
	{
		// The fake host records the call; no native window is opened.
		using var wb = new WebBrowser();
		var ex = Record.Exception(() => wb.Navigate(new Uri("https://example.com/")));
		Assert.Null(ex);
	});

	[Fact]
	public void WebBrowser_placeholder_label_updates_after_navigate() => Run(() =>
	{
		using var wb = new WebBrowser();
		_ = wb.Widget; // materialise
		wb.Navigate(new Uri("https://example.com/"));
		var label = (Gtk.Label)((Gtk.Box)wb.Widget).FirstChild!;
		Assert.Contains("example.com", label.Text);
	});

	[Fact]
	public void WebBrowser_Dispose_on_GTK_thread_does_not_throw() => Run(() =>
	{
		var wb = new WebBrowser();
		_ = wb.Widget;
		var ex = Record.Exception(() => wb.Dispose());
		Assert.Null(ex);
	});
}