using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>
/// GTK smoke tests for <see cref="DrawingBrush"/> assigned to shape fills.
/// Skipped automatically when the native GTK runtime is unavailable.
/// </summary>
public class DrawingBrushGtkSmokeTests : WpfGtkTestBase
{
	[Fact]
	public void Rectangle_with_DrawingBrush_has_DrawingArea_widget() => Run(() =>
	{
		var r = new Rectangle
		{
			Fill = new DrawingBrush
			{
				Drawing = new GeometryDrawing
				{
					Geometry = new RectangleGeometry(new Rect(0, 0, 40, 30)),
					Brush = new SolidColorBrush(Colors.Blue),
				},
			},
		};
		Assert.IsType<Gtk.DrawingArea>(r.Widget);
	});

	[Fact]
	public void Ellipse_with_DrawingBrush_has_DrawingArea_widget() => Run(() =>
	{
		var e = new Ellipse
		{
			Fill = new DrawingBrush
			{
				Drawing = new DrawingGroup(),
				Stretch = Stretch.Uniform,
			},
		};
		Assert.IsType<Gtk.DrawingArea>(e.Widget);
	});

	[Fact]
	public void DrawingBrush_null_drawing_does_not_throw_on_realize() => Run(() =>
	{
		var r = new Rectangle { Fill = new DrawingBrush() };
		var ex = Record.Exception(() =>
		{
			var area = (Gtk.DrawingArea)r.Widget;
			area.ContentWidth = 80;
			area.ContentHeight = 60;
		});
		Assert.Null(ex);
	});

	[Fact]
	public void DrawingBrush_dispose_does_not_throw() => Run(() =>
	{
		var r = new Rectangle
		{
			Fill = new DrawingBrush
			{
				Drawing = new GeometryDrawing { Brush = new SolidColorBrush(Colors.Green) },
			},
		};
		_ = r.Widget;
		var ex = Record.Exception(() => r.Dispose());
		Assert.Null(ex);
	});
}
