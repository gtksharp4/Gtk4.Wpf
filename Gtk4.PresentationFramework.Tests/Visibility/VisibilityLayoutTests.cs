using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.ElementVisibility;

/// <summary>Collapsed takes no space; Hidden keeps it.</summary>
public class VisibilityLayoutTests
{
	static readonly Size Inf = new Size(double.PositiveInfinity, double.PositiveInfinity);

	static VisElement Sized(double w, double h) => new VisElement { Width = w, Height = h };

	[Fact]
	public void Collapsed_child_has_zero_DesiredSize()
	{
		var el = Sized(50, 20);
		el.Visibility = Visibility.Collapsed;
		el.Measure(Inf);
		Assert.Equal(new Size(0, 0), el.DesiredSize);
	}

	[Fact]
	public void Hidden_child_keeps_its_DesiredSize()
	{
		var el = Sized(50, 20);
		el.Visibility = Visibility.Hidden;
		el.Measure(Inf);
		Assert.Equal(new Size(50, 20), el.DesiredSize);
	}

	[Fact]
	public void StackPanel_sizes_as_if_a_collapsed_child_were_absent()
	{
		var sp = new StackPanel();
		sp.Children.Add(Sized(40, 10));
		var gone = Sized(40, 30);
		gone.Visibility = Visibility.Collapsed;
		sp.Children.Add(gone);
		sp.Children.Add(Sized(40, 10));
		sp.Measure(Inf);
		Assert.Equal(20, sp.DesiredSize.Height);
	}

	[Fact]
	public void StackPanel_keeps_the_space_of_a_hidden_child()
	{
		var sp = new StackPanel();
		sp.Children.Add(Sized(40, 10));
		var hidden = Sized(40, 30);
		hidden.Visibility = Visibility.Hidden;
		sp.Children.Add(hidden);
		sp.Measure(Inf);
		Assert.Equal(40, sp.DesiredSize.Height);
	}

	[Fact]
	public void Toggling_Collapsed_re_measures_the_element_and_its_parent()
	{
		var sp = new StackPanel();
		var el = Sized(40, 10);
		sp.Children.Add(el);
		sp.Measure(Inf);
		Assert.Equal(10, sp.DesiredSize.Height);

		el.Visibility = Visibility.Collapsed;
		sp.Measure(Inf);
		Assert.Equal(0, sp.DesiredSize.Height);

		el.Visibility = Visibility.Visible;
		sp.Measure(Inf);
		Assert.Equal(10, sp.DesiredSize.Height);
	}

	[Fact]
	public void Collapsed_Arrange_skips_ArrangeCore_and_keeps_RenderSize()
	{
		var el = Sized(40, 10);
		el.Visibility = Visibility.Collapsed;
		el.Measure(Inf);
		el.Arrange(new Rect(0, 0, 100, 100));
		Assert.Equal(new Size(0, 0), el.RenderSize);
	}

	[Fact]
	public void Hidden_Arrange_runs_normally()
	{
		var el = Sized(40, 10);
		var reference = Sized(40, 10);
		reference.Measure(Inf);
		reference.Arrange(new Rect(0, 0, 100, 100));
		el.Visibility = Visibility.Hidden;
		el.Measure(Inf);
		el.Arrange(new Rect(0, 0, 100, 100));
		Assert.NotEqual(new Size(0, 0), el.RenderSize);
		Assert.Equal(reference.RenderSize, el.RenderSize);
	}
}
