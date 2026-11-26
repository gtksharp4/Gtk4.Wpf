using Xunit;
using WpfSlider = System.Windows.Controls.Slider;

namespace Gtk4.PresentationFramework.Tests.Controls;

public class SliderRangeSyncTests : WpfGtkTestBase
{
	[Fact]
	public void Value_survives_the_range_shrinking_and_growing_back() => Run(() =>
	{
		var slider = new WpfSlider { Maximum = 10, Value = 8 };
		var scale = (Gtk.Scale)slider.Widget;

		slider.Maximum = 5;
		Assert.Equal(5, slider.Value);

		slider.Maximum = 10;
		Assert.Equal(8, slider.Value);
		Assert.Equal(8, scale.Value);
	});
}
