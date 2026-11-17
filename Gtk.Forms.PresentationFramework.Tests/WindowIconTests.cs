using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk.Forms.PresentationFramework.Tests;

/// <summary>
/// The window icon is shown via a Gtk.HeaderBar titlebar (GTK4 has no per-window image icon),
/// matching the Gtk.Windows.Forms layer. Covers the headerbar install and the
/// ImageSource-to-path resolution behind it.
/// </summary>
public class WindowIconTests : WpfGtkTestBase
{
	// ── Pure: ImageSource -> file path ─────────────────────────────────────────

	[Fact]
	public void Null_icon_resolves_to_null()
	{
		Assert.Null(WpfWindow.ResolveIconPath(null));
	}

	[Fact]
	public void FilePath_image_source_resolves_to_its_path()
	{
		var src = ImageSource.FromFile(@"C:\app\logo.png");
		Assert.Equal(@"C:\app\logo.png", WpfWindow.ResolveIconPath(src));
	}

	[Fact]
	public void BitmapImage_absolute_uri_resolves_to_local_path()
	{
		var bmp = new BitmapImage(new Uri("file:///C:/app/logo.png", UriKind.Absolute));
		Assert.Equal(@"C:\app\logo.png", WpfWindow.ResolveIconPath(bmp));
	}

	[Fact]
	public void BitmapImage_relative_uri_resolves_to_original_string()
	{
		var bmp = new BitmapImage(new Uri("Assets/logo.png", UriKind.Relative));
		Assert.Equal("Assets/logo.png", WpfWindow.ResolveIconPath(bmp));
	}

	// ── GTK: titlebar hosts the icon ───────────────────────────────────────────

	[Fact]
	public void Window_titlebar_is_header_bar() => Run(() =>
	{
		var win = new WpfWindow { Title = "Iconned" };
		var gtkWin = (Gtk.Window)win.Widget;
		Assert.IsType<Gtk.HeaderBar>(gtkWin.Titlebar);
	});
}