using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;
using WpfImage = System.Windows.Controls.Image;

namespace Gtk4.PresentationFramework.Tests.Imaging;

/// <summary>WPF Image measure semantics: the size comes from the source, then Stretch and StretchDirection.</summary>
public class ImageDisplayLayoutTests : WpfGtkTestBase
{
	static readonly Size Infinite = new Size(double.PositiveInfinity, double.PositiveInfinity);

	static BitmapSource Bitmap(int w, int h, double dpi = 96) =>
		BitmapSource.Create(w, h, dpi, dpi, PixelFormats.Bgr24, null, new byte[w * 3 * h], w * 3);

	static Size Measure(ImageSource? source, Stretch stretch, Size available,
		StretchDirection direction = StretchDirection.Both)
	{
		var img = new WpfImage { Source = source, Stretch = stretch, StretchDirection = direction };
		img.Measure(available);
		return img.DesiredSize;
	}

	[Fact]
	public void Stretch_None_measures_the_natural_size_under_an_infinite_constraint() => Run(() =>
		Assert.Equal(new Size(128, 96), Measure(Bitmap(128, 96), Stretch.None, Infinite)));

	[Fact]
	public void Stretch_None_ignores_a_smaller_constraint() => Run(() =>
		Assert.Equal(new Size(128, 96), Measure(Bitmap(128, 96), Stretch.None, new Size(50, 50))));

	[Fact]
	public void The_natural_size_is_in_device_independent_units() => Run(() =>
		Assert.Equal(new Size(64, 32), Measure(Bitmap(128, 64, 192), Stretch.None, Infinite)));

	[Fact]
	public void A_null_source_measures_zero() => Run(() =>
		Assert.Equal(new Size(0, 0), Measure(null, Stretch.None, Infinite)));

	[Fact]
	public void Margin_adds_to_the_slot() => Run(() =>
	{
		var img = new WpfImage { Source = Bitmap(128, 128), Stretch = Stretch.None, Margin = new Thickness(20) };
		img.Measure(Infinite);
		Assert.Equal(new Size(168, 168), img.DesiredSize);
	});

	[Fact]
	public void Inside_a_StackPanel_the_slot_is_the_natural_size_plus_margin() => Run(() =>
	{
		var img = new WpfImage { Source = Bitmap(128, 128), Stretch = Stretch.None, Margin = new Thickness(20) };
		var panel = new StackPanel();
		panel.Children.Add(img);
		panel.Measure(new Size(300, 200));
		Assert.Equal(new Size(168, 168), img.DesiredSize);
		Assert.Equal(168, panel.DesiredSize.Height);
	});

	[Fact]
	public void Uniform_fits_inside_the_constraint_and_keeps_the_aspect() => Run(() =>
	{
		Assert.Equal(new Size(64, 32), Measure(Bitmap(128, 64), Stretch.Uniform, new Size(64, 64)));
		Assert.Equal(new Size(128, 64), Measure(Bitmap(128, 64), Stretch.Uniform, Infinite));
		Assert.Equal(new Size(256, 128), Measure(Bitmap(128, 64), Stretch.Uniform, new Size(256, 256)));
	});

	[Fact]
	public void Fill_takes_the_whole_constraint() => Run(() =>
		Assert.Equal(new Size(100, 50), Measure(Bitmap(128, 128), Stretch.Fill, new Size(100, 50))));

	[Fact]
	public void UniformToFill_covers_the_constraint() => Run(() =>
		Assert.Equal(new Size(100, 100), Measure(Bitmap(128, 128), Stretch.UniformToFill, new Size(100, 50))));

	[Fact]
	public void StretchDirection_limits_the_scaling() => Run(() =>
	{
		Assert.Equal(new Size(128, 128), Measure(Bitmap(128, 128), Stretch.Uniform, new Size(300, 300),
			StretchDirection.DownOnly));
		Assert.Equal(new Size(128, 128), Measure(Bitmap(128, 128), Stretch.Uniform, new Size(64, 64),
			StretchDirection.UpOnly));
	});

	[Fact]
	public void A_DrawingImage_measures_its_drawing_size() => Run(() =>
	{
		var drawing = new GeometryDrawing(Brushes.Red, null, new RectangleGeometry(new Rect(0, 0, 50, 40)));
		Assert.Equal(new Size(50, 40), Measure(new DrawingImage(drawing), Stretch.None, Infinite));
	});

	[Fact]
	public void An_empty_DrawingImage_and_a_file_source_measure_zero() => Run(() =>
	{
		Assert.Equal(new Size(0, 0), Measure(new DrawingImage(), Stretch.None, Infinite));
		Assert.Equal(new Size(0, 0), Measure(ImageSource.FromFile("missing.png"), Stretch.None, Infinite));
	});

	[Fact]
	public void Replacing_the_source_remeasures() => Run(() =>
	{
		var img = new WpfImage { Source = Bitmap(10, 10), Stretch = Stretch.None };
		img.Measure(Infinite);
		img.Source = Bitmap(30, 20);
		img.Measure(Infinite);
		Assert.Equal(new Size(30, 20), img.DesiredSize);
	});
}
