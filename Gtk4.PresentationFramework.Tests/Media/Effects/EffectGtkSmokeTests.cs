using System.Windows.Controls;
using System.Windows.Media.Effects;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media.Effects;

/// <summary>
/// GTK smoke tests for <see cref="System.Windows.UIElement.Effect"/>.
/// Skipped when GTK is unavailable.
/// </summary>
public class EffectGtkSmokeTests : WpfGtkTestBase
{
	[Fact]
	public void Effect_set_before_realize_applied_on_widget_creation()
	{
		Run(() =>
		{
			var tb = new TextBlock { Text = "blur" };
			tb.Effect = new BlurEffect { Radius = 4 };

			// Materialize — must not throw.
			var widget = tb.Widget;
			Assert.NotNull(widget);

			// DP value intact after widget creation.
			Assert.IsType<BlurEffect>(tb.Effect);
			Assert.Equal(4, ((BlurEffect)tb.Effect!).Radius);
		});
	}

	[Fact]
	public void Effect_set_after_realize_does_not_throw()
	{
		Run(() =>
		{
			var tb = new TextBlock { Text = "shadow" };
			_ = tb.Widget;

			var ex = Record.Exception(() =>
				tb.Effect = new DropShadowEffect { BlurRadius = 6, ShadowDepth = 4, Direction = 45 });
			Assert.Null(ex);
		});
	}

	[Fact]
	public void Effect_clear_after_realize_does_not_throw()
	{
		Run(() =>
		{
			var tb = new TextBlock { Text = "clear" };
			tb.Effect = new BlurEffect { Radius = 2 };
			_ = tb.Widget;

			var ex = Record.Exception(() => tb.Effect = null);
			Assert.Null(ex);
			Assert.Null(tb.Effect);
		});
	}
}
