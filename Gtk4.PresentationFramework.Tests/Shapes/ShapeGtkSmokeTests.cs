using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;
using WpfEllipse = System.Windows.Shapes.Ellipse;
using WpfRectangle = System.Windows.Shapes.Rectangle;

namespace Gtk4.PresentationFramework.Tests.Shapes;

/// <summary>
/// GTK smoke tests for shape elements: verify that the backing
/// <see cref="Gtk.DrawingArea"/> is created and can be realized without error.
/// All bodies run on the GTK thread via <c>Run(…)</c>.
/// Skipped when native GTK is unavailable.
/// </summary>
public class ShapeGtkSmokeTests : WpfGtkTestBase
{
	// ── Rectangle ────────────────────────────────────────────────────────────

	[Fact]
	public void Rectangle_Widget_is_DrawingArea() => Run(() =>
	{
		var r = new WpfRectangle();
		Assert.IsType<Gtk.DrawingArea>(r.Widget);
	});

	[Fact]
	public void Rectangle_with_Fill_creates_DrawingArea() => Run(() =>
	{
		var r = new WpfRectangle { Fill = Brushes.Red };
		Assert.IsType<Gtk.DrawingArea>(r.Widget);
	});

	[Fact]
	public void Rectangle_with_Fill_and_Stroke_creates_DrawingArea() => Run(() =>
	{
		var r = new WpfRectangle
		{
			Fill = Brushes.Blue,
			Stroke = Brushes.Black,
			StrokeThickness = 2
		};
		Assert.IsType<Gtk.DrawingArea>(r.Widget);
	});

	[Fact]
	public void Rectangle_realizing_widget_does_not_throw() => Run(() =>
	{
		var r = new WpfRectangle { Fill = Brushes.Green };
		var ex = Record.Exception(() =>
		{
			var area = (Gtk.DrawingArea)r.Widget;
			area.ContentWidth = 100;
			area.ContentHeight = 60;
		});
		Assert.Null(ex);
	});

	[Fact]
	public void Rectangle_with_radii_creates_DrawingArea() => Run(() =>
	{
		var r = new WpfRectangle { RadiusX = 8, RadiusY = 8, Fill = Brushes.Orange };
		Assert.IsType<Gtk.DrawingArea>(r.Widget);
	});

	// ── Ellipse ───────────────────────────────────────────────────────────────

	[Fact]
	public void Ellipse_Widget_is_DrawingArea() => Run(() =>
	{
		var e = new WpfEllipse();
		Assert.IsType<Gtk.DrawingArea>(e.Widget);
	});

	[Fact]
	public void Ellipse_with_Fill_creates_DrawingArea() => Run(() =>
	{
		var e = new WpfEllipse { Fill = Brushes.Red };
		Assert.IsType<Gtk.DrawingArea>(e.Widget);
	});

	[Fact]
	public void Ellipse_with_Stroke_only_creates_DrawingArea() => Run(() =>
	{
		var e = new WpfEllipse { Stroke = Brushes.Black, StrokeThickness = 3 };
		Assert.IsType<Gtk.DrawingArea>(e.Widget);
	});

	[Fact]
	public void Ellipse_realizing_widget_does_not_throw() => Run(() =>
	{
		var e = new WpfEllipse { Fill = Brushes.Cyan };
		var ex = Record.Exception(() =>
		{
			var area = (Gtk.DrawingArea)e.Widget;
			area.ContentWidth = 80;
			area.ContentHeight = 80;
		});
		Assert.Null(ex);
	});

	// ── Dispose ───────────────────────────────────────────────────────────────

	[Fact]
	public void Rectangle_Dispose_does_not_throw() => Run(() =>
	{
		var r = new WpfRectangle { Fill = Brushes.Red };
		_ = r.Widget; // materialise
		var ex = Record.Exception(() => r.Dispose());
		Assert.Null(ex);
	});

	[Fact]
	public void Ellipse_Dispose_does_not_throw() => Run(() =>
	{
		var e = new WpfEllipse { Fill = Brushes.Blue };
		_ = e.Widget;
		var ex = Record.Exception(() => e.Dispose());
		Assert.Null(ex);
	});
}