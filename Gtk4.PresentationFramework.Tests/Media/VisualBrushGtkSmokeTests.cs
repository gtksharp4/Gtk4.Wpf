using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>
/// GTK smoke tests for <see cref="VisualBrush"/> assigned to shape fills.
/// Skipped automatically when the native GTK runtime is unavailable.
/// </summary>
public class VisualBrushGtkSmokeTests : WpfGtkTestBase
{
	[Fact]
	public void Rectangle_with_VisualBrush_has_DrawingArea_widget() => Run(() =>
	{
		var dv = new DrawingVisual();
		using var scope = dv.RenderOpen();
		scope.Context.DrawRectangle(
			new SolidColorBrush(Colors.Blue), null, new Rect(0, 0, 40, 30));

		var r = new Rectangle { Fill = new VisualBrush(dv) };
		Assert.IsType<Gtk.DrawingArea>(r.Widget);
	});

	[Fact]
	public void Ellipse_with_VisualBrush_has_DrawingArea_widget() => Run(() =>
	{
		var dv = new DrawingVisual();
		using var scope = dv.RenderOpen();
		scope.Context.DrawEllipse(
			new SolidColorBrush(Colors.Green), null, new Point(20, 20), 20, 20);

		var e = new Ellipse
		{
			Fill = new VisualBrush(dv) { Stretch = Stretch.Uniform },
		};
		Assert.IsType<Gtk.DrawingArea>(e.Widget);
	});

	[Fact]
	public void VisualBrush_null_visual_does_not_throw_on_realize() => Run(() =>
	{
		var r = new Rectangle { Fill = new VisualBrush() };
		var ex = Record.Exception(() =>
		{
			var area = (Gtk.DrawingArea)r.Widget;
			area.ContentWidth = 80;
			area.ContentHeight = 60;
		});
		Assert.Null(ex);
	});

	[Fact]
	public void VisualBrush_DrawingVisual_no_committed_drawing_does_not_throw() => Run(() =>
	{
		var dv = new DrawingVisual();  // no RenderOpen/commit → no-op paint
		var r = new Rectangle { Fill = new VisualBrush(dv) };
		var ex = Record.Exception(() =>
		{
			var area = (Gtk.DrawingArea)r.Widget;
			area.ContentWidth = 80;
			area.ContentHeight = 60;
		});
		Assert.Null(ex);
	});

	[Fact]
	public void VisualBrush_general_Visual_does_not_throw_on_realize() => Run(() =>
	{
		// General UIElement: no-paint contract — must not throw.
		var elem = new System.Windows.Controls.Border();
		var r = new Rectangle { Fill = new VisualBrush(elem) };
		var ex = Record.Exception(() =>
		{
			var area = (Gtk.DrawingArea)r.Widget;
			area.ContentWidth = 80;
			area.ContentHeight = 60;
		});
		Assert.Null(ex);
	});

	[Fact]
	public void VisualBrush_dispose_does_not_throw() => Run(() =>
	{
		var dv = new DrawingVisual();
		using var scope = dv.RenderOpen();
		scope.Context.DrawRectangle(new SolidColorBrush(Colors.Orange), null, new Rect(0, 0, 50, 50));

		var r = new Rectangle { Fill = new VisualBrush(dv) };
		_ = r.Widget;
		var ex = Record.Exception(() => r.Dispose());
		Assert.Null(ex);
	});
}
