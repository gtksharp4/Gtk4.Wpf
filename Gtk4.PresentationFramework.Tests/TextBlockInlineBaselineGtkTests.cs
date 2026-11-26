using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Shapes;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Real-GTK: InlineUIContainer's default BaselineAlignment.Baseline.</summary>
public class TextBlockInlineBaselineGtkTests : WpfGtkTestBase
{
	static WpfWindow Show(TextBlock outer, FrameworkElement inner)
	{
		var win = new WpfWindow { Width = 600, Height = 400, Content = new StackPanel { Children = { outer } } };
		win.Show();
		GtkState.Require(() => inner.IsWidgetCreated && inner.Widget.IsMapped && inner.Widget.Width > 0,
			"inline child mapped", outer, inner);
		GtkPump.For(400);
		return win;
	}

	static TextBlock Build(FrameworkElement inner)
	{
		var outer = new TextBlock();
		outer.Inlines.Add(new Run("A "));
		outer.Inlines.Add(new InlineUIContainer(inner));
		outer.Inlines.Add(new Run(" B"));
		return outer;
	}

	[Fact]
	public void The_default_alignment_is_Baseline() => Run(() =>
		Assert.Equal(BaselineAlignment.Baseline, new InlineUIContainer().BaselineAlignment));

	[Fact]
	public void A_non_text_child_grows_the_line_and_its_bottom_sits_on_the_baseline() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var inner = new Rectangle { Width = 20, Height = 30 };
		var outer = Build(inner);
		var win = Show(outer, inner);
		try
		{
			Assert.True(outer.InlineFirstLineHeight >= 30, $"line height {outer.InlineFirstLineHeight}");
			// The child is placed by a later layout pass, which a slow runner may not have run yet.
			GtkPump.Until(() => inner.Widget.ComputeBounds(outer.Widget, out var r)
				&& System.Math.Abs(outer.InlineLabelBaseline - (r.Y + r.Height)) <= 1.0, 3000);
			Assert.True(inner.Widget.ComputeBounds(outer.Widget, out var b));
			Assert.Equal(outer.InlineLabelBaseline, b.Y + b.Height, 1.0);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void A_nested_TextBlock_aligns_its_first_line_baseline_with_the_outer_baseline() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var inner = new TextBlock { Text = "XYZ", FontSize = 30 };
		var outer = Build(inner);
		var win = Show(outer, inner);
		try
		{
			inner.Widget.Measure(Gtk.Orientation.Vertical, -1, out _, out _, out _, out int innerBaseline);
			Assert.True(inner.Widget.ComputeBounds(outer.Widget, out var b));
			Assert.Equal(outer.InlineLabelBaseline, b.Y + innerBaseline, 1.0);
		}
		finally { win.Close(); }
	});
}
