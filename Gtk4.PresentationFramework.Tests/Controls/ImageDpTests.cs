using System.Windows.Controls;
using System.Windows.Media;
using Xunit;
using WpfImage = System.Windows.Controls.Image;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure (no GTK) DP tests for Image.Stretch and Image.StretchDirection.
/// </summary>
public class ImageDpTests
{
	[Fact]
	public void Image_Stretch_default_is_Uniform()
	{
		var img = new WpfImage();
		Assert.Equal(Stretch.Uniform, img.Stretch);
	}

	[Fact]
	public void Image_Stretch_roundtrip_Fill()
	{
		var img = new WpfImage();
		img.Stretch = Stretch.Fill;
		Assert.Equal(Stretch.Fill, img.Stretch);
	}

	[Fact]
	public void Image_Stretch_roundtrip_None()
	{
		var img = new WpfImage();
		img.Stretch = Stretch.None;
		Assert.Equal(Stretch.None, img.Stretch);
	}

	[Fact]
	public void Image_Stretch_roundtrip_UniformToFill()
	{
		var img = new WpfImage();
		img.Stretch = Stretch.UniformToFill;
		Assert.Equal(Stretch.UniformToFill, img.Stretch);
	}

	[Fact]
	public void Image_StretchDirection_default_is_Both()
	{
		var img = new WpfImage();
		Assert.Equal(StretchDirection.Both, img.StretchDirection);
	}

	[Fact]
	public void Image_StretchDirection_roundtrip_UpOnly()
	{
		var img = new WpfImage();
		img.StretchDirection = StretchDirection.UpOnly;
		Assert.Equal(StretchDirection.UpOnly, img.StretchDirection);
	}

	[Fact]
	public void Image_StretchDirection_roundtrip_DownOnly()
	{
		var img = new WpfImage();
		img.StretchDirection = StretchDirection.DownOnly;
		Assert.Equal(StretchDirection.DownOnly, img.StretchDirection);
	}

	[Fact]
	public void Image_Stretch_set_before_realize_does_not_throw()
	{
		var img = new WpfImage();
		var ex = Record.Exception(() => img.Stretch = Stretch.Fill);
		Assert.Null(ex);
		Assert.Equal(Stretch.Fill, img.Stretch);
	}

	[Fact]
	public void Image_StretchDirection_set_before_realize_does_not_throw()
	{
		var img = new WpfImage();
		var ex = Record.Exception(() => img.StretchDirection = StretchDirection.DownOnly);
		Assert.Null(ex);
		Assert.Equal(StretchDirection.DownOnly, img.StretchDirection);
	}
}
