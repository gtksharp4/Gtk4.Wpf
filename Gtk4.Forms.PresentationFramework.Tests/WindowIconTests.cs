using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests;

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
	public void BitmapImage_relative_uri_resolves_under_the_app_directory()
	{
		var bmp = new BitmapImage(new Uri("Assets/logo.png", UriKind.Relative));
		Assert.Equal(Path.Combine(AppContext.BaseDirectory, "Assets", "logo.png"), WpfWindow.ResolveIconPath(bmp));
	}

	// ── GTK: titlebar hosts the icon ───────────────────────────────────────────

	[Fact]
	public void Window_titlebar_is_header_bar() => Run(() =>
	{
		var win = new WpfWindow { Title = "Iconned" };
		var gtkWin = (Gtk.Window)win.Widget;
		Assert.IsType<Gtk.HeaderBar>(gtkWin.Titlebar);
	});

	static Gtk.GestureClick? FindIconClickController(Gtk.Widget widget)
	{
		var controllers = widget.ObserveControllers();
		if (widget is Gtk.Image)
		{
			for (uint i = 0; i < controllers.NItems; i++)
				if (controllers.GetObject(i) is Gtk.GestureClick { Button: 1 } click)
					return click;
		}

		for (var child = widget.FirstChild; child != null; child = child.NextSibling)
			if (FindIconClickController(child) is { } click)
				return click;
		return null;
	}

	[Fact]
	public void Double_clicking_the_titlebar_icon_closes_the_window() => Run(() =>
	{
		var win = new WpfWindow { Title = "Iconned" };
		var gtkWin = (Gtk.Window)win.Widget;
		var headerBar = Assert.IsType<Gtk.HeaderBar>(gtkWin.Titlebar);
		var click = FindIconClickController(headerBar);
		Assert.NotNull(click);

		Assert.Equal(1u, click!.Button);

		int closed = 0;
		win.Closed += (_, _) => closed++;

		win.HandleTitleBarIconPress(1);
		Assert.Equal(0, closed);

		win.HandleTitleBarIconPress(2);
		Assert.Equal(1, closed);
	});
}
