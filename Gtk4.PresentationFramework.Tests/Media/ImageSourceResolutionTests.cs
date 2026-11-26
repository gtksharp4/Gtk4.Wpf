using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>ImageBrush and Window icon resolve BitmapImage URIs with the same rules as Image.</summary>
public class ImageSourceResolutionTests
{
	static string UnderBase(params string[] parts) =>
		Path.Combine(AppContext.BaseDirectory, Path.Combine(parts));

	[Fact]
	public void ImageBrush_relative_uri_resolves_under_the_base_directory()
	{
		var brush = new ImageBrush { ImageSource = new BitmapImage(new Uri("data/x.png", UriKind.Relative)) };
		Assert.Equal(UnderBase("data", "x.png"), brush.ResolvePath());
	}

	[Fact]
	public void ImageBrush_pack_uri_resolves_under_the_base_directory()
	{
		var uri = new Uri("pack://application/data/x.png", UriKind.Absolute);
		var brush = new ImageBrush { ImageSource = new BitmapImage(uri) };
		Assert.Equal(UnderBase("data", "x.png"), brush.ResolvePath());
	}

	[Fact]
	public void ImageBrush_without_source_resolves_to_null()
	{
		Assert.Null(new ImageBrush().ResolvePath());
	}

	[Fact]
	public void Window_icon_pack_uri_resolves_under_the_base_directory()
	{
		var icon = new BitmapImage(new Uri("pack://application/data/x.png", UriKind.Absolute));
		Assert.Equal(UnderBase("data", "x.png"), WpfWindow.ResolveIconPath(icon));
	}

	[Fact]
	public void Window_icon_from_a_converted_pack_string_resolves_under_the_base_directory()
	{
		var icon = System.Windows.Markup.UnitConverters.ParseImageSource("pack://application:,,,/data/x.png");
		Assert.Equal(UnderBase("data", "x.png"), WpfWindow.ResolveIconPath(icon));
	}
}
