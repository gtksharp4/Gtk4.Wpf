using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>
/// GTK smoke tests for <see cref="ImageBrush"/> assigned to shape fills.
/// Skipped automatically when the native GTK runtime is unavailable.
/// </summary>
public class ImageBrushGtkSmokeTests : WpfGtkTestBase
{
	[Fact]
	public void Rectangle_with_ImageBrush_has_DrawingArea_widget() => Run(() =>
	{
		var r = new Rectangle { Fill = new ImageBrush() };
		Assert.IsType<Gtk.DrawingArea>(r.Widget);
	});

	[Fact]
	public void Ellipse_with_ImageBrush_has_DrawingArea_widget() => Run(() =>
	{
		var e = new Ellipse { Fill = new ImageBrush { Stretch = Stretch.Uniform } };
		Assert.IsType<Gtk.DrawingArea>(e.Widget);
	});

	[Fact]
	public void ImageBrush_with_nonexistent_file_does_not_throw_on_realize() => Run(() =>
	{
		var r = new Rectangle
		{
			Fill = new ImageBrush
			{
				ImageSource = ImageSource.FromFile("missing_image.png"),
				Stretch = Stretch.Fill,
			}
		};
		var ex = Record.Exception(() =>
		{
			var area = (Gtk.DrawingArea)r.Widget;
			area.ContentWidth = 80;
			area.ContentHeight = 60;
		});
		Assert.Null(ex);
	});

	[Fact]
	public void ImageBrush_dispose_does_not_throw() => Run(() =>
	{
		var r = new Rectangle { Fill = new ImageBrush() };
		_ = r.Widget;
		var ex = Record.Exception(() => r.Dispose());
		Assert.Null(ex);
	});
}
