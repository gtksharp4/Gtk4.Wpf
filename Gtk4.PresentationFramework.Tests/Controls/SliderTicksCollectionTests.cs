using System.Linq;
using System.Windows.Media;
using Xunit;
using WpfSlider = System.Windows.Controls.Slider;
using WpfTickPlacement = System.Windows.Controls.Primitives.TickPlacement;

namespace Gtk4.PresentationFramework.Tests.Controls;

public class SliderTicksCollectionTests
{
	[Fact]
	public void Ticks_defaults_to_null()
	{
		Assert.Null(new WpfSlider().Ticks);
		Assert.Equal(typeof(DoubleCollection), WpfSlider.TicksProperty.PropertyType);
	}

	[Fact]
	public void Ticks_round_trips()
	{
		var ticks = new DoubleCollection { 1, 5 };
		var s = new WpfSlider { Ticks = ticks };
		Assert.Same(ticks, s.Ticks);
	}

	[Fact]
	public void Explicit_ticks_replace_the_frequency_positions()
	{
		var s = new WpfSlider { Maximum = 10, TickFrequency = 1, Ticks = new DoubleCollection { 2, 7.5, 20 } };
		Assert.Equal(new[] { 2.0, 7.5 }, s.TickValues().ToArray());
	}

	[Fact]
	public void Empty_ticks_fall_back_to_frequency()
	{
		var s = new WpfSlider { Maximum = 4, TickFrequency = 2, Ticks = new DoubleCollection() };
		Assert.Equal(new[] { 0.0, 2.0, 4.0 }, s.TickValues().ToArray());
	}

	[Fact]
	public void Setting_ticks_before_realize_does_not_throw()
	{
		var s = new WpfSlider { TickPlacement = WpfTickPlacement.Both, Ticks = new DoubleCollection { 3 } };
		Assert.Single(s.Ticks!);
	}
}
