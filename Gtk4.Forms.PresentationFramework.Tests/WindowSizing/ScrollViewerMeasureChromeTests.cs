using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.WindowSizing;

/// <summary>
/// WPF ScrollViewer desired size adds Padding (ScrollContentPresenter margin), BorderThickness and a Visible scroll
/// bar before clamping to the constraint (R68E-2).
/// </summary>
public class ScrollViewerMeasureChromeTests : WpfGtkTestBase
{
	static readonly Size Infinite = new Size(double.PositiveInfinity, double.PositiveInfinity);

	static ScrollViewer Viewer(ScrollBarVisibility h, ScrollBarVisibility v, UIElement? content = null) =>
		new ScrollViewer
		{
			Content = content ?? new Border { Width = 300, Height = 200 },
			HorizontalScrollBarVisibility = h,
			VerticalScrollBarVisibility = v,
		};

	[Fact]
	public void Padding_adds_to_the_desired_size() => Run(() =>
	{
		var sv = Viewer(ScrollBarVisibility.Auto, ScrollBarVisibility.Auto);
		sv.Padding = new Thickness(1, 2, 3, 4);
		sv.Measure(Infinite);
		Assert.Equal(new Size(304, 206), sv.DesiredSize);
	});

	[Fact]
	public void BorderThickness_adds_to_the_desired_size() => Run(() =>
	{
		var sv = Viewer(ScrollBarVisibility.Auto, ScrollBarVisibility.Auto);
		sv.BorderThickness = new Thickness(2, 3, 4, 5);
		sv.Measure(Infinite);
		Assert.Equal(new Size(306, 208), sv.DesiredSize);
	});

	[Fact]
	public void A_Visible_vertical_bar_adds_its_width() => Run(() =>
	{
		var sv = Viewer(ScrollBarVisibility.Auto, ScrollBarVisibility.Visible);
		sv.Measure(Infinite);
		Assert.Equal(new Size(300 + SystemParameters.VerticalScrollBarWidth, 200), sv.DesiredSize);
	});

	[Fact]
	public void A_Visible_horizontal_bar_adds_its_height() => Run(() =>
	{
		var sv = Viewer(ScrollBarVisibility.Visible, ScrollBarVisibility.Auto);
		sv.Measure(Infinite);
		Assert.Equal(new Size(300, 200 + SystemParameters.HorizontalScrollBarHeight), sv.DesiredSize);
	});

	[Fact]
	public void Auto_Hidden_and_Disabled_bars_add_nothing() => Run(() =>
	{
		var sv = Viewer(ScrollBarVisibility.Hidden, ScrollBarVisibility.Disabled);
		sv.Measure(Infinite);
		Assert.Equal(new Size(300, 200), sv.DesiredSize);
	});

	[Fact]
	public void The_chrome_is_added_before_clamping() => Run(() =>
	{
		var sv = Viewer(ScrollBarVisibility.Auto, ScrollBarVisibility.Visible);
		sv.Padding = new Thickness(10);
		sv.Measure(new Size(250, 100));
		Assert.Equal(new Size(250, 100), sv.DesiredSize);
	});

	[Fact]
	public void A_Disabled_axis_gives_the_content_the_constraint_minus_the_chrome() => Run(() =>
	{
		var probe = new Probe();
		var sv = Viewer(ScrollBarVisibility.Disabled, ScrollBarVisibility.Visible, probe);
		sv.Padding = new Thickness(5);
		sv.Measure(new Size(200, 100));
		Assert.Equal(200 - 10 - SystemParameters.VerticalScrollBarWidth, probe.Last.Width);
		Assert.True(double.IsPositiveInfinity(probe.Last.Height));
	});

	sealed class Probe : FrameworkElement
	{
		public Size Last;

		protected override Size MeasureOverride(Size availableSize)
		{
			Last = availableSize;
			return new Size(10, 10);
		}
	}
}
