using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.WindowSizing;

/// <summary>WPF ScrollViewer.MeasureOverride: widen the constraint on scrollable axes, clamp the result.</summary>
public class ScrollViewerMeasureTests : WpfGtkTestBase
{
	static readonly Size Infinite = new Size(double.PositiveInfinity, double.PositiveInfinity);

	static ScrollViewer Viewer(UIElement? content, ScrollBarVisibility h = ScrollBarVisibility.Auto,
		ScrollBarVisibility v = ScrollBarVisibility.Auto) =>
		new ScrollViewer { Content = content, HorizontalScrollBarVisibility = h, VerticalScrollBarVisibility = v };

	[Fact]
	public void Infinite_constraint_gives_the_content_size() => Run(() =>
	{
		var sv = Viewer(new Border { Width = 300, Height = 200 });
		sv.Measure(Infinite);
		Assert.Equal(new Size(300, 200), sv.DesiredSize);
	});

	[Fact]
	public void A_smaller_constraint_clamps_the_desired_size() => Run(() =>
	{
		var sv = Viewer(new Border { Width = 300, Height = 200 });
		sv.Measure(new Size(100, 100));
		Assert.Equal(new Size(100, 100), sv.DesiredSize);
	});

	[Fact]
	public void Null_content_measures_zero() => Run(() =>
	{
		var sv = Viewer(null);
		sv.Measure(Infinite);
		Assert.Equal(new Size(0, 0), sv.DesiredSize);
	});

	[Fact]
	public void A_scrollable_axis_measures_its_content_unconstrained() => Run(() =>
	{
		var probe = new MeasureProbe();
		var sv = Viewer(probe);
		sv.Measure(new Size(100, 80));
		Assert.Equal(new Size(double.PositiveInfinity, double.PositiveInfinity), probe.Last);
	});

	[Fact]
	public void A_Disabled_horizontal_axis_keeps_the_constraint() => Run(() =>
	{
		var probe = new MeasureProbe();
		var sv = Viewer(probe, h: ScrollBarVisibility.Disabled);
		sv.Measure(new Size(100, 80));
		Assert.Equal(new Size(100, double.PositiveInfinity), probe.Last);
	});

	[Fact]
	public void A_Disabled_vertical_axis_keeps_the_constraint() => Run(() =>
	{
		var probe = new MeasureProbe();
		var sv = Viewer(probe, v: ScrollBarVisibility.Disabled);
		sv.Measure(new Size(100, 80));
		Assert.Equal(new Size(double.PositiveInfinity, 80), probe.Last);
	});

	[Fact]
	public void Hidden_and_Visible_axes_still_scroll() => Run(() =>
	{
		var probe = new MeasureProbe();
		Viewer(probe, ScrollBarVisibility.Hidden, ScrollBarVisibility.Visible).Measure(new Size(100, 80));
		Assert.Equal(new Size(double.PositiveInfinity, double.PositiveInfinity), probe.Last);
	});

	[Fact]
	public void Content_smaller_than_the_constraint_keeps_its_size() => Run(() =>
	{
		var sv = Viewer(new Border { Width = 30, Height = 20 });
		sv.Measure(new Size(100, 100));
		Assert.Equal(new Size(30, 20), sv.DesiredSize);
	});

	[Fact]
	public void Padding_is_part_of_the_desired_size_only_through_Margin() => Run(() =>
	{
		var sv = Viewer(new Border { Width = 300, Height = 200 });
		sv.Margin = new Thickness(5);
		sv.Measure(Infinite);
		Assert.Equal(new Size(310, 210), sv.DesiredSize);
	});

	sealed class MeasureProbe : FrameworkElement
	{
		public Size Last;

		protected override Size MeasureOverride(Size availableSize)
		{
			Last = availableSize;
			return new Size(300, 200);
		}
	}
}
