using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;
using WpfRectangle = System.Windows.Shapes.Rectangle;

namespace Gtk4.PresentationFramework.Tests.Shapes;

/// <summary>
/// GTK smoke tests for stroke-style properties: verifies that setting dash arrays,
/// caps and joins on a realized shape does not throw.
/// All bodies run on the GTK thread via <c>Run(…)</c>.
/// Skipped when native GTK is unavailable.
/// </summary>
public class StrokeStyleGtkSmokeTests : WpfGtkTestBase
{
	[Fact]
	public void Rectangle_with_dash_array_draws_without_error() => Run(() =>
	{
		var r = new WpfRectangle
		{
			Stroke = Brushes.Black,
			StrokeThickness = 2,
			StrokeDashArray = new DoubleCollection { 4, 2 },
		};
		var ex = Record.Exception(() =>
		{
			var area = (Gtk.DrawingArea)r.Widget;
			area.ContentWidth  = 100;
			area.ContentHeight = 60;
		});
		Assert.Null(ex);
	});

	[Fact]
	public void Rectangle_with_round_caps_and_bevel_join_draws_without_error() => Run(() =>
	{
		var r = new WpfRectangle
		{
			Stroke = Brushes.Red,
			StrokeThickness = 3,
			StrokeStartLineCap = PenLineCap.Round,
			StrokeEndLineCap = PenLineCap.Round,
			StrokeLineJoin = PenLineJoin.Bevel,
		};
		var ex = Record.Exception(() => _ = r.Widget);
		Assert.Null(ex);
	});

	[Fact]
	public void Rectangle_with_miter_limit_draws_without_error() => Run(() =>
	{
		var r = new WpfRectangle
		{
			Stroke = Brushes.Blue,
			StrokeThickness = 2,
			StrokeLineJoin = PenLineJoin.Miter,
			StrokeMiterLimit = 4.0,
		};
		var ex = Record.Exception(() => _ = r.Widget);
		Assert.Null(ex);
	});

	[Fact]
	public void Rectangle_with_full_stroke_style_draws_without_error() => Run(() =>
	{
		var r = new WpfRectangle
		{
			Stroke = Brushes.Green,
			StrokeThickness = 2,
			StrokeDashArray = new DoubleCollection { 3, 1, 1, 1 },
			StrokeDashOffset = 0.5,
			StrokeDashCap = PenDashCap.Round,
			StrokeStartLineCap = PenLineCap.Square,
			StrokeEndLineCap = PenLineCap.Square,
			StrokeLineJoin = PenLineJoin.Round,
			StrokeMiterLimit = 10.0,
		};
		var ex = Record.Exception(() =>
		{
			var area = (Gtk.DrawingArea)r.Widget;
			area.ContentWidth  = 150;
			area.ContentHeight = 80;
		});
		Assert.Null(ex);
	});
}
