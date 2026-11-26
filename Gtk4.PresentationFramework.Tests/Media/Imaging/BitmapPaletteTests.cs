using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

public class BitmapPaletteTests
{
	[Fact]
	public void Constructor_validates_the_colour_list()
	{
		Assert.Throws<ArgumentNullException>(() => new BitmapPalette((IList<Color>)null!));
		Assert.Throws<ArgumentException>(() => new BitmapPalette(new List<Color>()));
		Assert.Throws<ArgumentException>(() => new BitmapPalette(Enumerable.Repeat(Colors.Red, 257).ToList()));
		Assert.Equal(256, new BitmapPalette(Enumerable.Repeat(Colors.Red, 256).ToList()).Colors.Count);
	}

	[Fact]
	public void Colors_are_read_only_and_copied()
	{
		var source = new List<Color> { Colors.Red, Colors.Blue };
		var palette = new BitmapPalette(source);
		source.Clear();
		Assert.Equal(2, palette.Colors.Count);
		Assert.True(palette.Colors.IsReadOnly);
		Assert.Equal(Colors.Blue, palette.Colors[1]);
	}

	[Theory]
	[InlineData("BlackAndWhite", 2, "FF000000", "FFFFFFFF")]
	[InlineData("BlackAndWhiteTransparent", 2, "FF000000", "FFFFFFFF")]
	[InlineData("Gray4", 4, "FF000000", "FFFFFFFF")]
	[InlineData("Gray16", 16, "FF000000", "FFFFFFFF")]
	[InlineData("Gray256", 256, "FF000000", "FFFFFFFF")]
	[InlineData("Halftone8", 16, "FF000000", "FF008080")]
	[InlineData("Halftone8Transparent", 17, "FF000000", "00000000")]
	[InlineData("Halftone27", 28, "FF000000", "FFC0C0C0")]
	[InlineData("Halftone27Transparent", 29, "FF000000", "00000000")]
	[InlineData("Halftone64", 72, "FF000000", "FF008080")]
	[InlineData("Halftone125", 126, "FF000000", "FFC0C0C0")]
	[InlineData("Halftone216", 224, "FF000000", "FF008080")]
	[InlineData("Halftone252", 252, "FF000000", "FFFFFFFF")]
	[InlineData("Halftone252Transparent", 253, "FF000000", "00000000")]
	[InlineData("Halftone256", 256, "FF000000", "FFFFFFFF")]
	[InlineData("Halftone256Transparent", 256, "FF000000", "00000000")]
	[InlineData("WebPalette", 224, "FF000000", "FF008080")]
	[InlineData("WebPaletteTransparent", 225, "FF000000", "00000000")]
	public void Fixed_palettes_match_the_wic_tables(string name, int count, string first, string last)
	{
		var palette = (BitmapPalette)typeof(BitmapPalettes).GetProperty(name)!.GetValue(null)!;
		Assert.Equal(count, palette.Colors.Count);
		Assert.Equal(first, Hex(palette.Colors[0]));
		Assert.Equal(last, Hex(palette.Colors[count - 1]));
	}

	[Fact]
	public void Halftone_tables_have_the_wic_ordering()
	{
		var h8 = BitmapPalettes.Halftone8.Colors;
		Assert.Equal("FF0000FF", Hex(h8[1]));
		Assert.Equal("FF00FF00", Hex(h8[2]));
		Assert.Equal("FFFF0000", Hex(h8[4]));
		Assert.Equal("FFC0C0C0", Hex(h8[8]));
		Assert.Equal("FF002B00", Hex(BitmapPalettes.Halftone252.Colors[6]));
		Assert.Equal("FF002400", Hex(BitmapPalettes.Halftone256.Colors[4]));
	}

	[Fact]
	public void Palette_of_a_bitmap_holds_its_colours()
	{
		var pixels = new byte[] { 255, 0, 0, 255, 0, 255, 0, 255 };
		var source = BitmapSource.Create(2, 1, 96, 96, PixelFormats.Bgra32, null, pixels, 8);
		var palette = new BitmapPalette(source, 256);
		Assert.Equal(2, palette.Colors.Count);
		Assert.Throws<ArgumentNullException>(() => new BitmapPalette((BitmapSource)null!, 4));
	}

	static string Hex(Color c) => $"{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
}
