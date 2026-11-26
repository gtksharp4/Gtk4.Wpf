using System.Linq;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

public class PixelFormatTests
{
	[Theory]
	[InlineData("Indexed1", 1, "01")]
	[InlineData("Indexed2", 2, "03")]
	[InlineData("Indexed4", 4, "0F")]
	[InlineData("Indexed8", 8, "FF")]
	[InlineData("BlackWhite", 1, "01")]
	[InlineData("Gray2", 2, "03")]
	[InlineData("Gray4", 4, "0F")]
	[InlineData("Gray8", 8, "FF")]
	[InlineData("Gray16", 16, "FFFF")]
	[InlineData("Gray32Float", 32, "FFFFFFFF")]
	[InlineData("Bgr555", 16, "1F00,E003,007C")]
	[InlineData("Bgr565", 16, "1F00,E007,00F8")]
	[InlineData("Bgr24", 24, "FF0000,00FF00,0000FF")]
	[InlineData("Rgb24", 24, "FF0000,00FF00,0000FF")]
	[InlineData("Bgr101010", 32, "FF030000,00FC0F00,0000F03F")]
	[InlineData("Bgr32", 32, "FF000000,00FF0000,0000FF00")]
	[InlineData("Bgra32", 32, "FF000000,00FF0000,0000FF00,000000FF")]
	[InlineData("Pbgra32", 32, "FF000000,00FF0000,0000FF00,000000FF")]
	[InlineData("Rgb48", 48, "FFFF00000000,0000FFFF0000,00000000FFFF")]
	[InlineData("Rgba64", 64, "FFFF000000000000,0000FFFF00000000,00000000FFFF0000,000000000000FFFF")]
	[InlineData("Prgba64", 64, "FFFF000000000000,0000FFFF00000000,00000000FFFF0000,000000000000FFFF")]
	[InlineData("Cmyk32", 32, "FF000000,00FF0000,0000FF00,000000FF")]
	public void Members_match_the_wic_values(string name, int bpp, string masks)
	{
		var format = (PixelFormat)typeof(PixelFormats).GetField(name)!.GetValue(null)!;
		Assert.Equal(name, format.ToString());
		Assert.Equal(bpp, format.BitsPerPixel);
		var actual = format.Masks.Select(m => string.Concat(m.Mask.Select(b => b.ToString("X2"))));
		Assert.Equal(masks, string.Join(",", actual));
	}

	[Fact]
	public void Float_formats_and_default()
	{
		Assert.Equal(128, PixelFormats.Rgba128Float.BitsPerPixel);
		Assert.Equal(4, PixelFormats.Prgba128Float.Masks.Count);
		Assert.Equal(3, PixelFormats.Rgb128Float.Masks.Count);
		Assert.Equal("Default", PixelFormats.Default.ToString());
		Assert.Empty(PixelFormats.Default.Masks);
	}

	[Fact]
	public void Equality_is_by_format_identity()
	{
		var same = PixelFormats.Bgra32;
		Assert.True(PixelFormats.Bgra32 == same);
		Assert.True(PixelFormats.Bgra32 != PixelFormats.Pbgra32);
		Assert.Equal(PixelFormats.Gray8.GetHashCode(), PixelFormats.Gray8.GetHashCode());
	}
}
