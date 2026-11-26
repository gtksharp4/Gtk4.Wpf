using System;
using System.IO;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Imaging;

/// <summary>A decoder frame of each codec, shown like the imaging samples, draws its pixels.</summary>
public class ImageDisplayCodecTests : WpfGtkTestBase
{
	public static TheoryData<string, bool> Cases()
	{
		var data = new TheoryData<string, bool>();
		foreach (var codec in new[] { "BMP", "GIF", "JPEG", "PNG", "TIFF" })
		{
			data.Add(codec, false);
			data.Add(codec, true);
		}
		return data;
	}

	static void ShowAndCheck(string codec, BitmapSource frame)
	{
		Assert.Equal(ImageDisplayFixtures.Size, frame.PixelWidth);
		using var probe = new ImageDisplayProbe(frame);
		Assert.Equal(ImageDisplayFixtures.Size, probe.Image.ActualWidth);
		Assert.Equal(ImageDisplayFixtures.Size, probe.Image.ActualHeight);
		ImageDisplayFixtures.AssertQuadrants(probe.ImagePixels(), codec == "JPEG" ? 48 : 8);
	}

	[Theory]
	[MemberData(nameof(Cases))]
	public void Frame_decoded_from_a_stream_or_an_absolute_file_uri_is_drawn(string codec, bool fromUri) => Run(() =>
	{
		byte[] data = ImageDisplayFixtures.Encode(codec, ImageDisplayFixtures.Quadrant());
		if (!fromUri)
		{
			using var stream = new MemoryStream(data);
			ShowAndCheck(codec, ImageDisplayFixtures.Decode(codec, stream).Frames[0]);
			return;
		}
		string path = Path.Combine(Path.GetTempPath(), "lane-e-" + Guid.NewGuid().ToString("N") + "."
			+ ImageDisplayFixtures.Extension(codec));
		File.WriteAllBytes(path, data);
		try { ShowAndCheck(codec, ImageDisplayFixtures.Decode(codec, new Uri(path)).Frames[0]); }
		finally { File.Delete(path); }
	});

	[Theory]
	[InlineData("BMP")]
	[InlineData("GIF")]
	[InlineData("JPEG")]
	[InlineData("PNG")]
	[InlineData("TIFF")]
	public void Frame_decoded_from_a_relative_uri_next_to_the_app_is_drawn(string codec) => Run(() =>
	{
		string name = "lane-e-" + Guid.NewGuid().ToString("N") + "." + ImageDisplayFixtures.Extension(codec);
		string path = Path.Combine(AppContext.BaseDirectory, name);
		File.WriteAllBytes(path, ImageDisplayFixtures.Encode(codec, ImageDisplayFixtures.Quadrant()));
		try
		{
			var decoder = ImageDisplayFixtures.Decode(codec, new Uri(name, UriKind.RelativeOrAbsolute));
			ShowAndCheck(codec, decoder.Frames[0]);
		}
		finally { File.Delete(path); }
	});
}
