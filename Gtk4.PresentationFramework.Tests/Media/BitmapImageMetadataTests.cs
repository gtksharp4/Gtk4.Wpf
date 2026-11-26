using System;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

public class BitmapImageMetadataTests
{
	[Fact]
	public void BitmapImage_Metadata_throws_NotSupportedException_as_in_WPF()
	{
		var image = new BitmapImage();
		Assert.Throws<NotSupportedException>(() => image.Metadata);
	}

	[Fact]
	public void BitmapFrame_over_a_bitmap_still_reads_null_metadata()
	{
		var src = BitmapSource.Create(1, 1, 96, 96, System.Windows.Media.PixelFormats.Gray8, null, new byte[1], 1);
		Assert.Null(src.Metadata);
	}
}
