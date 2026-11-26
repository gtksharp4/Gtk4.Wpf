using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Lifecycle;

/// <summary>
/// The size-changed chain: <see cref="UIElement.Arrange"/> only queues SizeChanged; the queue
/// runs after the outermost arrange pass, top-down (WPF's prepended
/// <c>ContextLayoutManager</c> chain), so every handler sees final sizes.
/// </summary>
public class SizeChangedChainTests
{
	/// <summary>Arranges its single child twice within one ArrangeOverride.</summary>
	sealed class DoubleArrangePanel : StackPanel
	{
		protected override Size ArrangeOverride(Size finalSize)
		{
			var child = Children[0];
			child.Arrange(new Rect(0, 0, 10, 10));
			child.Arrange(new Rect(0, 0, 20, 30));
			return finalSize;
		}
	}

	static void Layout(UIElement el, double w, double h)
	{
		el.Measure(new Size(w, h));
		el.Arrange(new Rect(0, 0, w, h));
	}

	[Fact]
	public void Nested_SizeChanged_fires_parent_first_after_the_whole_pass()
	{
		var root = new StackPanel { Name = "root" };
		var child = new Border { Name = "child" };
		root.Children.Add(child);
		var log = new List<string>();
		root.SizeChanged += (_, _) => log.Add("root child.ActualWidth=" + child.ActualWidth
			+ " dp=" + child.GetValue(FrameworkElement.ActualWidthProperty));
		child.SizeChanged += (_, _) => log.Add("child");

		Layout(root, 100, 50);

		// Parent first; the child was already arranged (final RenderSize) when the parent's handler ran,
		// but its DP is only published by its own (later) notification — as in WPF.
		Assert.Equal(new[] { "root child.ActualWidth=100 dp=0", "child" }, log);
	}

	[Fact]
	public void Siblings_fire_after_their_parent()
	{
		var root = new StackPanel();
		var a = new Border();
		var b = new Border();
		root.Children.Add(a);
		root.Children.Add(b);
		var log = new List<string>();
		root.SizeChanged += (_, _) => log.Add("root");
		a.SizeChanged += (_, _) => log.Add("a");
		b.SizeChanged += (_, _) => log.Add("b");

		Layout(root, 100, 50);

		Assert.Equal(3, log.Count);
		Assert.Equal("root", log[0]);
		Assert.Contains("a", log);
		Assert.Contains("b", log);
	}

	[Fact]
	public void Handler_sees_later_siblings_already_arranged()
	{
		var root = new StackPanel();
		var first = new Border();
		var second = new Border();
		root.Children.Add(first);
		root.Children.Add(second);
		double secondWidthSeenByFirst = -1;
		first.SizeChanged += (_, _) => secondWidthSeenByFirst = second.ActualWidth;

		Layout(root, 80, 40);

		// Raised after the whole pass, not mid-pass before `second` was arranged.
		Assert.Equal(80, secondWidthSeenByFirst);
	}

	[Fact]
	public void Element_arranged_twice_in_one_pass_raises_one_merged_SizeChanged()
	{
		var panel = new DoubleArrangePanel();
		var child = new Border();
		panel.Children.Add(child);
		var events = new List<SizeChangedEventArgs>();
		child.SizeChanged += (_, e) => events.Add(e);

		Layout(panel, 100, 100);

		var only = Assert.Single(events);
		Assert.Equal(new Size(0, 0), only.PreviousSize);
		Assert.Equal(new Size(20, 30), only.NewSize);
		Assert.True(only.WidthChanged && only.HeightChanged);
	}

	[Fact]
	public void Handler_that_re_arranges_starts_a_new_chain()
	{
		var b = new Border();
		var sizes = new List<double>();
		b.SizeChanged += (_, e) =>
		{
			sizes.Add(e.NewSize.Width);
			if (e.NewSize.Width == 50)
				b.Arrange(new Rect(0, 0, 70, 10));
		};

		Layout(b, 50, 10);

		Assert.Equal(new[] { 50.0, 70.0 }, sizes);
		Assert.Equal(70, b.ActualWidth);
	}
}
