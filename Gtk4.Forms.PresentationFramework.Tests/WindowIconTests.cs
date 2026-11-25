using System;
using System.Globalization;
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
	public void Icon_is_a_dependency_property()
	{
		var window = new WpfWindow();
		var icon = ImageSource.FromFile(@"C:\app\logo.png");

		window.Icon = icon;

		Assert.Same(icon, window.GetValue(WpfWindow.IconProperty));
		Assert.Same(icon, window.Icon);
	}

	[Fact]
	public void Default_icon_resource_is_extracted_to_a_temporary_file()
	{
		string path = WpfWindow.ExtractDefaultIconToTempFile();
		try
		{
			Assert.StartsWith(Path.GetTempPath(), path, StringComparison.OrdinalIgnoreCase);
			using var resource = typeof(WpfWindow).Assembly.GetManifestResourceStream(
				"Gtk4.Forms.PresentationFramework.GtkSharp.png");
			Assert.NotNull(resource);
			using var expected = new MemoryStream();
			resource.CopyTo(expected);
			Assert.Equal(expected.ToArray(), File.ReadAllBytes(path));
		}
		finally
		{
			File.Delete(path);
		}
	}

	[Fact]
	public void Default_icon_loads_from_the_embedded_resource() => Run(() =>
		Assert.NotNull(WpfWindow.LoadDefaultIconImage()));

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

	static Gtk.Image? FindTitlebarIcon(Gtk.Widget widget)
	{
		if (widget is Gtk.Image { } image && image.HasCssClass("titlebar-icon"))
			return image;

		for (var child = widget.FirstChild; child != null; child = child.NextSibling)
			if (FindTitlebarIcon(child) is { } foundIcon)
				return foundIcon;
		return null;
	}

	[Theory]
	[InlineData("it-IT", "Chiudi finestra")]
	[InlineData("fr-FR", "Fermer la fenêtre")]
	[InlineData("de-DE", "Fenster schließen")]
	[InlineData("es-ES", "Cerrar ventana")]
	[InlineData("pt-PT", "Fechar janela")]
	[InlineData("en-US", "Close Window")]
	[InlineData("ja-JP", "Close Window")]
	public void Close_window_menu_label_uses_the_requested_language(string cultureName, string expected) =>
		Assert.Equal(expected, WpfWindow.GetCloseWindowMenuLabel(CultureInfo.GetCultureInfo(cultureName)));

	[Fact]
	public void Double_clicking_the_titlebar_icon_closes_the_window() => Run(() =>
	{
		var win = new WpfWindow { Title = "Iconned" };
		var gtkWin = (Gtk.Window)win.Widget;
		var headerBar = Assert.IsType<Gtk.HeaderBar>(gtkWin.Titlebar);
		var icon = FindTitlebarIcon(headerBar);
		Assert.NotNull(icon);
		Assert.True(icon!.HasCssClass("titlebar-icon"));

		var controllers = icon.ObserveControllers();
		Gtk.GestureClick? click = null;
		for (uint i = 0; i < controllers.NItems; i++)
			if (controllers.GetObject(i) is Gtk.GestureClick gesture && gesture.Button == 1)
				click = gesture;
		Assert.NotNull(click);

		int closed = 0;
		win.Closed += (_, _) => closed++;

		win.HandleTitleBarIconPress(1);
		Assert.Equal(0, closed);
		Assert.True(win.TitleBarIconContextMenu!.Visible);

		win.HandleTitleBarIconPress(2);
		Assert.Equal(1, closed);
	});

	[Fact]
	public void Right_clicking_the_titlebar_icon_has_a_close_menu_that_closes_the_window() => Run(() =>
	{
		var win = new WpfWindow { Title = "Iconned" };
		var gtkWin = (Gtk.Window)win.Widget;
		var headerBar = Assert.IsType<Gtk.HeaderBar>(gtkWin.Titlebar);
		var icon = FindTitlebarIcon(headerBar);
		Assert.NotNull(icon);

		var controllers = icon!.ObserveControllers();
		Gtk.GestureClick? contextClick = null;
		for (uint i = 0; i < controllers.NItems; i++)
			if (controllers.GetObject(i) is Gtk.GestureClick gesture && gesture.Button == 3)
				contextClick = gesture;
		Assert.NotNull(contextClick);

		var popover = Assert.IsType<Gtk.Popover>(win.TitleBarIconContextMenu);
		Assert.Same(icon, popover.Parent);
		var closeButton = Assert.IsType<Gtk.Button>(popover.Child);
		Assert.Equal(WpfWindow.GetCloseWindowMenuLabel(CultureInfo.CurrentUICulture), closeButton.Label);

		int closed = 0;
		win.Closed += (_, _) => closed++;

		win.HandleTitleBarIconContextMenuClose();
		Assert.Equal(1, closed);
	});
}
