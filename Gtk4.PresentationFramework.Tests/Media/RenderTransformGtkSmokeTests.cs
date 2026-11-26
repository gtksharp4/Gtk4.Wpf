using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>
/// GTK smoke tests for <see cref="UIElement.RenderTransform"/>.
/// Skipped when GTK is unavailable.
/// </summary>
public class RenderTransformGtkSmokeTests : WpfGtkTestBase
{
	[Fact]
	public void RenderTransform_set_before_realize_applied_on_widget_creation()
	{
		Run(() =>
		{
			var tb = new TextBlock { Text = "hi" };
			tb.RenderTransform = new TranslateTransform(10, 20);

			// Materialize the widget — must not throw.
			var widget = tb.Widget;
			Assert.NotNull(widget);

			// The DP value must still be intact after widget creation.
			var m = tb.RenderTransform!.Value;
			Assert.Equal(10, m.OffsetX, 1e-9);
			Assert.Equal(20, m.OffsetY, 1e-9);
		});
	}

	[Fact]
	public void RenderTransform_set_after_realize_does_not_throw()
	{
		Run(() =>
		{
			var tb = new TextBlock { Text = "test" };
			// Realize first
			_ = tb.Widget;
			// Now change the transform — must not throw.
			tb.RenderTransform = new RotateTransform(45);
			Assert.True(tb.RenderTransform!.Value.M11 < 1); // cos(45) ≈ 0.707
		});
	}

	[Fact]
	public void RenderTransformOrigin_set_does_not_throw()
	{
		Run(() =>
		{
			var tb = new TextBlock { Text = "origin" };
			_ = tb.Widget;
			tb.RenderTransformOrigin = new Point(0.5, 0.5);
			Assert.Equal(0.5, tb.RenderTransformOrigin.X);
		});
	}

	[Fact]
	public void TransformGroup_on_realized_widget_does_not_throw()
	{
		Run(() =>
		{
			var tb = new TextBlock { Text = "group" };
			_ = tb.Widget;
			var g = new TransformGroup();
			g.Children.Add(new ScaleTransform(2, 2));
			g.Children.Add(new TranslateTransform(5, 5));
			tb.RenderTransform = g;
			// Composed matrix: scale(2,2) → scale point (1,1) to (2,2) → translate → (7,7)
			var p = tb.RenderTransform!.Value.Transform(new Point(1, 1));
			Assert.Equal(7, p.X, 1e-9);
			Assert.Equal(7, p.Y, 1e-9);
		});
	}
}
