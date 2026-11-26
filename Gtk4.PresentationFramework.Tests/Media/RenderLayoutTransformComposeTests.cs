using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>
/// Pins the Round-22 fix: LayoutTransform and RenderTransform must COMPOSE into one CSS
/// matrix rather than overwrite each other (they share a single per-element CSS provider).
/// Verifies <c>UIElement.ComposeLayoutAndRender</c> directly (no display needed).
/// </summary>
public class RenderLayoutTransformComposeTests
{
	const double Eps = 1e-6;

	[Fact]
	public void Both_transforms_compose_not_overwrite()
	{
		var layout = new ScaleTransform(2, 2);
		var render = new ScaleTransform(3, 3);

		Matrix m = UIElement.ComposeLayoutAndRender(layout, render);

		// 2× then 3× → 6× on both axes. If one overwrote the other it would be 2 or 3.
		Assert.Equal(6, m.M11, Eps);
		Assert.Equal(6, m.M22, Eps);
	}

	[Fact]
	public void Null_layout_yields_render_only()
	{
		var render = new ScaleTransform(3, 3);
		Matrix m = UIElement.ComposeLayoutAndRender(null, render);
		Assert.Equal(3, m.M11, Eps);
		Assert.Equal(3, m.M22, Eps);
	}

	[Fact]
	public void Null_render_yields_layout_only()
	{
		var layout = new ScaleTransform(4, 4);
		Matrix m = UIElement.ComposeLayoutAndRender(layout, null);
		Assert.Equal(4, m.M11, Eps);
		Assert.Equal(4, m.M22, Eps);
	}

	[Fact]
	public void Both_null_is_identity()
	{
		Matrix m = UIElement.ComposeLayoutAndRender(null, null);
		Assert.True(m.IsIdentity);
	}

	[Fact]
	public void Layout_applied_before_render_translate_then_scale()
	{
		// Layout translates (+10,+20); render scales 2×. Left-to-right (layout first):
		// combined = Identity · translate · scale. Offsets scale by the later matrix.
		var layout = new TranslateTransform(10, 20);
		var render = new ScaleTransform(2, 2);

		Matrix m = UIElement.ComposeLayoutAndRender(layout, render);

		Assert.Equal(2, m.M11, Eps);
		Assert.Equal(2, m.M22, Eps);
		Assert.Equal(20, m.OffsetX, Eps);
		Assert.Equal(40, m.OffsetY, Eps);
	}
}
