using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace System.Windows;

public partial class Window
{
	const int IconSize = 24;
	const uint IconHoverCssPriority = 601;
	const string IconHoverCss = ".titlebar-icon:hover { filter: drop-shadow(0px 2px 3px rgba(0,0,0,0.65)); }";

	// GTK4 has no exe-icon equivalent; a generic application icon from the bundled theme is the
	// cross-platform default shown when the app sets no Icon.
	const string DefaultIconName = "application-x-executable";

	Gtk.HeaderBar? _headerBar;
	Gtk.Image? _iconImage;
	Gtk.CssProvider? _iconHoverCssProvider;

	/// <summary>The WPF-compatible window icon, shown in the GTK HeaderBar titlebar.</summary>
	public static readonly DependencyProperty IconProperty =
		DependencyProperty.Register(
			nameof(Icon),
			typeof(ImageSource),
			typeof(Window),
			new PropertyMetadata(null, OnIconChanged));

	static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var window = (Window)d;
		if (window._headerBar != null)
			window.UpdateHeaderBarIcon();
	}

	/// <summary>
	/// Gets or sets the window icon. When unset, a default application icon is shown.
	/// Setting a new value updates the live titlebar.
	/// </summary>
	public ImageSource? Icon
	{
		get => (ImageSource?)GetValue(IconProperty);
		set => SetValue(IconProperty, value);
	}

	/// <summary>
	/// Installs a <see cref="Gtk.HeaderBar"/> titlebar (keeping the standard window buttons) so
	/// the window icon can be shown. GTK4 removed per-window image icons, so — matching the
	/// Gtk.Windows.Forms layer — the icon is a <see cref="Gtk.Image"/> packed into the header
	/// bar, which works on Windows, Linux and macOS alike.
	/// </summary>
	void InstallHeaderBar(Gtk.Window win)
	{
		_headerBar = new Gtk.HeaderBar(); // ShowTitleButtons defaults true: keep min/max/close
		win.Titlebar = _headerBar;
		UpdateHeaderBarIcon();
	}

	/// <summary>Replaces the header-bar icon image so repeated sets do not stack.</summary>
	void UpdateHeaderBarIcon()
	{
		if (_headerBar == null)
			return;

		if (_iconImage != null)
		{
			if (_iconHoverCssProvider != null)
				_iconImage.StyleContext.RemoveProvider(_iconHoverCssProvider);
			_headerBar.Remove(_iconImage);
			_iconImage = null;
			_iconHoverCssProvider = null;
		}

		_iconImage = CreateIconImage();
		if (_iconImage != null)
			_headerBar.PackStart(_iconImage);
	}

	Gtk.Image? CreateIconImage()
	{
		Gtk.Image? img = LoadIconImage(Icon) ?? Gtk.Image.NewFromIconName(DefaultIconName);
		if (img != null)
		{
			// Explicit size requests make the header-bar image take space and render.
			img.WidthRequest = IconSize;
			img.HeightRequest = IconSize;
			img.Visible = true;
			img.AddCssClass("titlebar-icon");

			_iconHoverCssProvider = new Gtk.CssProvider();
			_iconHoverCssProvider.LoadFromData(IconHoverCss);
			img.StyleContext.AddProvider(_iconHoverCssProvider, IconHoverCssPriority);

			var click = new Gtk.GestureClick { Button = 1 };
			click.Pressed += (_, e) => HandleTitleBarIconPress(e.NPress);
			img.AddController(click);
		}
		return img;
	}

	internal void HandleTitleBarIconPress(int nPress)
	{
		if (nPress == 2)
			Close();
	}

	/// <summary>Loads <paramref name="icon"/> as a scaled image, or null when it has no file.</summary>
	static Gtk.Image? LoadIconImage(ImageSource? icon)
	{
		string? path = ResolveIconPath(icon);
		if (string.IsNullOrEmpty(path) || !File.Exists(path))
			return null;

		try
		{
			var pixbuf = new Gdk.Pixbuf(path);
			var scaled = pixbuf.ScaleSimple(IconSize, IconSize, Gdk.InterpType.Bilinear);
			return new Gtk.Image(scaled);
		}
		catch
		{
			// An unreadable icon is cosmetic — fall back to the default.
			return null;
		}
	}

	/// <summary>Resolves an <see cref="ImageSource"/> to a local file path, or null.</summary>
	internal static string? ResolveIconPath(ImageSource? icon)
	{
		switch (icon)
		{
			case null:
				return null;
			case BitmapImage bmp when bmp.UriSource != null:
				return ImageSourceLocation.ResolveUri(bmp.UriSource);
			default:
				var fp = icon.FilePath;
				return string.IsNullOrEmpty(fp) ? null : fp;
		}
	}
}