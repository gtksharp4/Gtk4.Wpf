using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

/// <summary>
/// Pins the WPF type hierarchy and shapes of <see cref="BitmapImage"/>/<see cref="BitmapSource"/>
/// that consumer code relies on (e.g. <c>using System.Windows.Media.Imaging;</c> and
/// <c>ImageSource img = new BitmapImage(uri);</c>).
/// </summary>
public class BitmapImageTests
{
	[Fact]
	public void BitmapImage_is_an_ImageSource()
	{
		// WPF hierarchy: BitmapImage : BitmapSource : ImageSource.
		Assert.IsAssignableFrom<BitmapSource>(new BitmapImage());
		Assert.IsAssignableFrom<ImageSource>(new BitmapImage());
	}

	[Fact]
	public void Uri_constructor_sets_UriSource()
	{
		var uri = new Uri("https://example.test/img.png");
		var bmp = new BitmapImage(uri);

		Assert.Same(uri, bmp.UriSource);
	}

	[Fact]
	public void BeginInit_set_EndInit_round_trips_UriSource()
	{
		var uri = new Uri("https://example.test/logo.png");
		var bmp = new BitmapImage();

		bmp.BeginInit();
		bmp.UriSource = uri;
		bmp.EndInit();

		Assert.Same(uri, bmp.UriSource);
	}
}