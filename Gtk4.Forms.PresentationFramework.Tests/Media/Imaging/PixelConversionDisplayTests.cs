using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Media.Imaging;

/// <summary>
/// An opaque source never converts to alpha 0 (fully transparent) for the formats decoders and samples use.
/// </summary>
public class PixelConversionDisplayTests
{
	static BitmapPalette Palette => new BitmapPalette(new[] { Colors.Red, Colors.Blue, Colors.Green });

	public static TheoryData<string, byte[], int> Opaque() => new TheoryData<string, byte[], int>
	{
		{ "Indexed1", new byte[] { 0x00 }, 1 },
		{ "Indexed2", new byte[] { 0x00 }, 1 },
		{ "Indexed4", new byte[] { 0x00 }, 1 },
		{ "Indexed8", new byte[] { 0x00 }, 1 },
		{ "Gray8", new byte[] { 0x80 }, 1 },
		{ "Bgr24", new byte[] { 1, 2, 3 }, 3 },
		{ "Rgb24", new byte[] { 1, 2, 3 }, 3 },
		{ "Bgr32", new byte[] { 1, 2, 3, 0 }, 4 },
		{ "Bgra32", new byte[] { 1, 2, 3, 255 }, 4 },
		{ "Pbgra32", new byte[] { 1, 2, 3, 255 }, 4 },
		{ "Cmyk32", new byte[] { 0, 0, 0, 0 }, 4 },
		{ "Rgb48", new byte[] { 1, 0, 2, 0, 3, 0 }, 6 },
		{ "Rgba64", new byte[] { 1, 0, 2, 0, 3, 0, 255, 255 }, 8 },
	};

	[Theory]
	[MemberData(nameof(Opaque))]
	public void Opaque_pixels_convert_to_alpha_255(string format, byte[] pixel, int stride)
	{
		var member = typeof(PixelFormats).GetMember(format)[0];
		object? value = member is System.Reflection.FieldInfo fi
			? fi.GetValue(null)
			: ((System.Reflection.PropertyInfo)member).GetValue(null);
		var f = (PixelFormat)value!;
		var palette = format.StartsWith("Indexed") ? Palette : null;
		var source = BitmapSource.Create(1, 1, 96, 96, f, palette, pixel, stride);
		byte[] bgra = source.ToBgra32();
		Assert.Equal(4, bgra.Length);
		Assert.Equal(255, bgra[3]);
	}

	[Fact]
	public void Indexed1_palette_index_0_is_opaque_red()
	{
		var source = BitmapSource.Create(8, 1, 96, 96, PixelFormats.Indexed1, Palette, new byte[] { 0x00 }, 1);
		byte[] bgra = source.ToBgra32();
		Assert.Equal(new byte[] { 0, 0, 255, 255 }, bgra[..4]);
	}

	[Fact]
	public void Pbgra32_converts_back_to_straight_alpha()
	{
		var source = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Pbgra32, null, new byte[] { 0, 0, 128, 128 }, 4);
		byte[] bgra = source.ToBgra32();
		Assert.Equal(128, bgra[3]);
		Assert.InRange(bgra[2], 254, 255);
	}
}
