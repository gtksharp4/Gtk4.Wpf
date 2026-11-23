using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Building a tree full of ElementName bindings must stay linear in the number of bindings.</summary>
public class ElementNameScalingTests : WpfGtkTestBase
{
	static double BuildMilliseconds(int items)
	{
		var root = new StackPanel();
		var head = new TextBlock { Text = "h" };
		root.Children.Add(head);
		root.RegisterName("head", head);
		var watch = Stopwatch.StartNew();
		for (int i = 0; i < items; i++)
		{
			var border = new Border();
			var stack = new StackPanel();
			var bound = new TextBlock();
			bound.SetBinding(TextBlock.TextProperty, new Binding("Text") { ElementName = "head" });
			stack.Children.Add(bound);
			stack.Children.Add(new TextBlock());
			stack.Children.Add(new TextBlock());
			border.Child = stack;
			root.Children.Add(border);
			Assert.Equal("h", bound.Text);
		}
		return watch.Elapsed.TotalMilliseconds;
	}

	static double Best(int items)
	{
		var best = double.MaxValue;
		for (int run = 0; run < 3; run++)
			best = System.Math.Min(best, BuildMilliseconds(items));
		return best;
	}

	[Fact]
	public void Building_a_tree_with_ElementName_bindings_scales_linearly() => Run(() =>
	{
		Best(100);
		double small = Best(500), large = Best(2000);
		Assert.True(large / small < 6, $"t(2000)/t(500) = {large / small:F1} ({small:F0} ms, {large:F0} ms)");
	});
}
