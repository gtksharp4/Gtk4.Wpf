using System;
using System.Windows;
using System.Windows.Controls;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Gtk4.Forms.PresentationFramework.Tests.XamlBacked;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>The KeyFrameAnimation sample's listing page, compiled from its XAML (xml:space=preserve + nested blocks).</summary>
public class KeyFrameListingGtkTests : WpfGtkTestBase
{
	static TextBlock[] Names(KeyFrameListingPage p) => new[]
	{
		p.FirstLinearTextBlock, p.FirstSplineTextBlock, p.SecondLinearTextBlock,
		p.ThirdLinearTextBlock, p.SecondSplineTextBlock,
	};

	static (WpfWindow, KeyFrameListingPage) Show()
	{
		var page = new KeyFrameListingPage();
		var win = new WpfWindow { Width = 900, Height = 800, Content = page };
		win.Show();
		var first = page.FirstLinearTextBlock;
		GtkState.Require(() => first.IsWidgetCreated && first.Widget.IsMapped && first.Widget.Width > 0,
			"nested block mapped", first);
		GtkPump.For(100);
		return (win, page);
	}

	[Fact]
	public void Nested_blocks_are_in_the_name_scope_and_the_logical_tree() => Run(() =>
	{
		var page = new KeyFrameListingPage();
		foreach (var (name, tb) in new[]
		{
			("FirstLinearTextBlock", page.FirstLinearTextBlock), ("FirstSplineTextBlock", page.FirstSplineTextBlock),
			("SecondLinearTextBlock", page.SecondLinearTextBlock),
			("ThirdLinearTextBlock", page.ThirdLinearTextBlock),
			("SecondSplineTextBlock", page.SecondSplineTextBlock),
		})
		{
			Assert.NotNull(tb);
			Assert.Same(tb, page.FindName(name));
			var container = Assert.IsType<System.Windows.Documents.InlineUIContainer>(LogicalTreeHelper.GetParent(tb));
			Assert.IsType<TextBlock>(LogicalTreeHelper.GetParent(container));
		}
	});

	[Fact]
	public void The_nested_blocks_sit_on_their_own_lines_at_the_listing_indent() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (win, page) = Show();
		try
		{
			var blocks = Names(page);
			var outer = (TextBlock)LogicalTreeHelper.GetParent(
				LogicalTreeHelper.GetParent(blocks[0])!)!;
			double lastY = -1, x0 = double.NaN;
			foreach (var tb in blocks)
			{
				GtkState.Require(() => tb.Widget.Width > 0 && tb.Widget.IsMapped, "mapped", tb);
				Assert.True(tb.Widget.ComputeBounds(outer.Widget, out var b));
				Assert.True(b.Y > lastY + 5, $"{tb.Name} Y {b.Y} after {lastY}");
				lastY = b.Y;
				if (double.IsNaN(x0)) x0 = b.X;
				Assert.Equal(x0, b.X, 1.0);
				Assert.True(b.X > 20, $"indent {b.X}");
			}
			Assert.True(outer.Widget.Height >= 8 * 14, $"eight lines, height {outer.Widget.Height}");
		}
		finally { win.Close(); }
	});

	static void Advance(double seconds)
	{
		for (double t = 0; t < seconds - 1e-9; t += 0.05)
		{
			System.Windows.Media.Animation.AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(0.05));
			GtkPump.For(5);
		}
	}

	[Fact]
	public void The_Loaded_storyboard_enlarges_each_block_in_turn_and_moves_the_rectangle() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (win, page) = Show();
		try
		{
			var first = page.FirstLinearTextBlock;
			var second = page.FirstSplineTextBlock;
			int h12 = first.Widget.Height;
			double y2 = YOf(page, second);
			Advance(0.5);
			Assert.Equal(16, first.FontSize, 0.1);
			Assert.Equal(12, second.FontSize, 0.1);
			Assert.True(Canvas.GetLeft(page.myRectangle) > 40, "rectangle moved");
			GtkState.Require(() => first.Widget.Height > h12, "first block taller", first);
			Assert.True(YOf(page, second) > y2, "the next block moved down");
			Advance(4.8);
			GtkState.Require(() => first.Widget.Height == h12 && second.FontSize > 15.9, "swap at 5s", first, second);
			Advance(5.6);
			Assert.Equal(16, page.SecondLinearTextBlock.FontSize, 0.1);
			Assert.Equal(12, second.FontSize, 0.1);
		}
		finally { win.Close(); }
	});

	static double YOf(KeyFrameListingPage page, TextBlock tb)
	{
		var outer = (TextBlock)LogicalTreeHelper.GetParent(LogicalTreeHelper.GetParent(tb)!)!;
		tb.Widget.ComputeBounds(outer.Widget, out var b);
		return b.Y;
	}
}
