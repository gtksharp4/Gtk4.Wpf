using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Real-GTK: UIElements inside a TextBlock's inlines are laid out inline with the text.</summary>
public class TextBlockInlineUIContainerGtkTests : WpfGtkTestBase
{
	static (WpfWindow Window, TextBlock Outer) Show(TextBlock outer, TextBlock inner)
	{
		var win = new WpfWindow { Width = 600, Height = 400, Content = new StackPanel { Children = { outer } } };
		win.Show();
		GtkState.Require(() => inner.IsWidgetCreated && inner.Widget.IsMapped && inner.Widget.Width > 0,
			"inline child mapped", outer, inner);
		GtkPump.For(400);
		return (win, outer);
	}

	static double ChildX(TextBlock outer, TextBlock inner)
	{
		Assert.True(inner.Widget.ComputeBounds(outer.Widget, out var b));
		return b.X;
	}

	static TextBlock Build(string before, TextBlock inner, string after, out InlineUIContainer container)
	{
		var outer = new TextBlock();
		outer.Inlines.Add(new Run(before));
		container = new InlineUIContainer(inner);
		outer.Inlines.Add(container);
		outer.Inlines.Add(new Run(after));
		return outer;
	}

	[Fact]
	public void A_nested_TextBlock_is_laid_out_between_the_surrounding_runs() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var inner = new TextBlock { Text = "XYZ" };
		var outer = Build("a ", inner, " b", out _);
		var (win, _) = Show(outer, inner);
		try
		{
			Assert.True(inner.Widget.Width > 0 && inner.Widget.Height > 0);
			Assert.True(inner.Widget.IsMapped);
			var (before, after) = outer.InlineObjectEdges(0);
			Assert.True(before > 0, "the text before the child has width");
			Assert.InRange(ChildX(outer, inner), before - 1, before + 1);
			Assert.InRange(before + inner.Widget.Width, after - 1, after + 1);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void A_preserved_newline_before_the_child_moves_it_to_the_second_line() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var inner = new TextBlock { Text = "XYZ" };
		var outer = Build("a\n", inner, " b", out _);
		var (win, _) = Show(outer, inner);
		try
		{
			Assert.True(inner.Widget.ComputeBounds(outer.Widget, out var b));
			Assert.True(b.Y >= outer.InlineFirstLineHeight - 1, $"child Y {b.Y} on line 2");
			Assert.Equal(0, ChildX(outer, inner), 1);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Raising_the_child_FontSize_grows_its_box_and_pushes_the_following_text_right() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var inner = new TextBlock { Text = "XYZ", FontSize = 12 };
		var outer = Build("a ", inner, " b", out _);
		var (win, _) = Show(outer, inner);
		try
		{
			int h12 = inner.Widget.Height, w12 = inner.Widget.Width;
			double outerH12 = outer.Widget.Height;
			double after12 = outer.InlineObjectEdges(0).After;
			inner.FontSize = 36;
			GtkState.Require(() => inner.Widget.Height > h12, "child grew", outer, inner);
			GtkPump.For(400);
			Assert.True(inner.Widget.Width > w12);
			Assert.True(outer.Widget.Height > outerH12, "outer TextBlock grew with the child");
			Assert.True(outer.InlineObjectEdges(0).After > after12, "following text moved right");
			inner.FontSize = 12;
			GtkState.Require(() => inner.Widget.Height == h12, "child back to base size", outer, inner);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void A_collapsed_child_reserves_no_width() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var inner = new TextBlock { Text = "XYZ" };
		var outer = Build("a ", inner, " b", out _);
		var (win, _) = Show(outer, inner);
		try
		{
			double visible = outer.InlineObjectEdges(0).After - outer.InlineObjectEdges(0).Before;
			Assert.True(visible > 0);
			inner.Visibility = Visibility.Collapsed;
			GtkState.Require(() => outer.InlineObjectEdges(0).After - outer.InlineObjectEdges(0).Before < 1,
				"zero-width box", outer, inner);
		}
		finally { win.Close(); }
	});
}

public class InlineStoryboardGtkTests : WpfGtkTestBase
{
	[Fact]
	public void A_named_nested_TextBlock_FontSize_is_driven_by_a_Storyboard() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var inner = new TextBlock { Text = "XYZ", FontSize = 12 };
		var outer = new TextBlock();
		outer.Inlines.Add(new Run("a "));
		outer.Inlines.Add(new InlineUIContainer(inner));
		outer.Inlines.Add(new Run(" b"));
		var panel = new StackPanel { Children = { outer } };
		var page = new System.Windows.Controls.Page { Content = panel };
		page.RegisterName("nested", inner);
		var anim = new System.Windows.Media.Animation.DoubleAnimation(12, 16, TimeSpan.FromSeconds(1));
		System.Windows.Media.Animation.Storyboard.SetTargetName(anim, "nested");
		System.Windows.Media.Animation.Storyboard.SetTargetProperty(anim, new PropertyPath("FontSize"));
		var board = new System.Windows.Media.Animation.Storyboard();
		board.Children.Add(anim);
		var win = new WpfWindow { Width = 600, Height = 400, Content = page };
		win.Show();
		GtkState.Require(() => inner.IsWidgetCreated && inner.Widget.IsMapped && inner.Widget.Width > 0,
			"mapped", outer, inner);
		try
		{
			int h12 = inner.Widget.Height;
			board.Begin(panel, true);
			board.Seek(panel, TimeSpan.FromSeconds(0.5), System.Windows.Media.Animation.TimeSeekOrigin.BeginTime);
			Assert.Equal(14, inner.FontSize, 0.5);
			GtkState.Require(() => inner.Widget.Height > h12, "child grew with the animated font", outer, inner);
			board.Seek(panel, TimeSpan.FromSeconds(2), System.Windows.Media.Animation.TimeSeekOrigin.BeginTime);
			Assert.Equal(16, inner.FontSize, 0.1);
		}
		finally { win.Close(); }
	});
}

public class StoryboardSamePropertyTests : WpfGtkTestBase
{
	// The KeyFrameAnimation sample grows and later shrinks one TextBlock with two animations of one Storyboard.
	[Fact]
	public void Animations_of_one_Storyboard_on_the_same_property_layer_instead_of_replacing() => Run(() =>
	{
		var target = new TextBlock { Text = "x", FontSize = 12 };
		var panel = new StackPanel { Children = { target } };
		var grow = new System.Windows.Media.Animation.DoubleAnimation(12, 16, TimeSpan.FromSeconds(0.2));
		var shrink = new System.Windows.Media.Animation.DoubleAnimation(16, 12, TimeSpan.FromSeconds(0.2))
		{
			BeginTime = TimeSpan.FromSeconds(5),
		};
		var board = new System.Windows.Media.Animation.Storyboard();
		foreach (var a in new[] { grow, shrink })
		{
			System.Windows.Media.Animation.Storyboard.SetTarget(a, target);
			System.Windows.Media.Animation.Storyboard.SetTargetProperty(a, new PropertyPath("FontSize"));
			board.Children.Add(a);
		}
		board.Begin(panel, true);
		var seek = System.Windows.Media.Animation.TimeSeekOrigin.BeginTime;
		board.Seek(panel, TimeSpan.FromSeconds(1), seek);
		Assert.Equal(16, target.FontSize, 0.01);
		board.Seek(panel, TimeSpan.FromSeconds(5.1), seek);
		Assert.Equal(14, target.FontSize, 0.1);
		board.Seek(panel, TimeSpan.FromSeconds(6), seek);
		Assert.Equal(12, target.FontSize, 0.01);
	});
}
