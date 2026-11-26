using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>A Shape applies its Clip while painting; the pixels show it.</summary>
public class ShapeClipPixelTests : WpfGtkTestBase
{
	const int Size = 20;

	static (byte B, byte G, byte R, byte A) PixelAt(Cairo.ImageSurface surface, int x, int y)
	{
		surface.Flush();
		byte[] data = surface.Data;
		int i = y * surface.Stride + x * 4;
		return (data[i], data[i + 1], data[i + 2], data[i + 3]);
	}

	static Cairo.ImageSurface Paint(Rectangle rectangle)
	{
		var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, Size, Size);
		using var cr = new Cairo.Context(surface);
		rectangle.PaintTo(cr, Size, Size);
		return surface;
	}

	[Fact]
	public void A_clipped_rectangle_paints_only_inside_the_clip() => Run(() =>
	{
		var rectangle = new Rectangle { Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0)) };
		rectangle.Clip = new RectangleGeometry(new Rect(0, 0, Size / 2, Size));
		using var surface = Paint(rectangle);

		var inside = PixelAt(surface, 2, 10);
		Assert.Equal((byte)255, inside.R);
		Assert.Equal((byte)255, inside.A);
		var outside = PixelAt(surface, Size - 2, 10);
		Assert.NotEqual((byte)255, outside.R);
		Assert.Equal((byte)0, outside.A);
	});

	[Fact]
	public void Without_a_clip_the_whole_rectangle_is_painted() => Run(() =>
	{
		var rectangle = new Rectangle { Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0)) };
		using var surface = Paint(rectangle);
		Assert.Equal((byte)255, PixelAt(surface, Size - 2, 10).R);
	});
}
