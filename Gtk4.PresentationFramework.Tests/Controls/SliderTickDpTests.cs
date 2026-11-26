using Xunit;
using WpfSlider        = System.Windows.Controls.Slider;
using WpfOrientation   = System.Windows.Controls.Orientation;
using WpfTickPlacement = System.Windows.Controls.Primitives.TickPlacement;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure (no GTK) DP default / round-trip tests for the Slider tick properties,
/// plus the snap-on-Value coercion. (Snap-math helpers are in <c>SliderSnapTests</c>.)
/// </summary>
public class SliderTickDpTests
{
	// ── DP defaults ──────────────────────────────────────────────────────────

	[Fact]
	public void Slider_SmallChange_and_LargeChange_keep_the_RangeBase_defaults()
	{
		var s = new WpfSlider();
		Assert.Equal(0.1, s.SmallChange);
		Assert.Equal(1.0, s.LargeChange);
		Assert.Equal(0.1, (double)WpfSlider.SmallChangeProperty.GetMetadata(typeof(WpfSlider)).DefaultValue!);
		Assert.Equal(10.0, s.Maximum);
	}

	[Fact]
	public void Slider_TickFrequency_default_is_1()
	{
		Assert.Equal(1.0, new WpfSlider().TickFrequency);
	}

	[Fact]
	public void Slider_TickPlacement_default_is_None()
	{
		Assert.Equal(WpfTickPlacement.None, new WpfSlider().TickPlacement);
	}

	[Fact]
	public void Slider_IsSnapToTickEnabled_default_is_false()
	{
		Assert.False(new WpfSlider().IsSnapToTickEnabled);
	}

	[Fact]
	public void Slider_IsDirectionReversed_default_is_false()
	{
		Assert.False(new WpfSlider().IsDirectionReversed);
	}

	// ── DP round-trips ───────────────────────────────────────────────────────

	[Fact]
	public void Slider_SmallChange_roundtrip()
	{
		var s = new WpfSlider { SmallChange = 5.0 };
		Assert.Equal(5.0, s.SmallChange);
	}

	[Fact]
	public void Slider_LargeChange_roundtrip()
	{
		var s = new WpfSlider { LargeChange = 25.0 };
		Assert.Equal(25.0, s.LargeChange);
	}

	[Fact]
	public void Slider_TickFrequency_roundtrip()
	{
		var s = new WpfSlider { TickFrequency = 5.0 };
		Assert.Equal(5.0, s.TickFrequency);
	}

	[Fact]
	public void Slider_TickPlacement_roundtrip_BottomRight()
	{
		var s = new WpfSlider { TickPlacement = WpfTickPlacement.BottomRight };
		Assert.Equal(WpfTickPlacement.BottomRight, s.TickPlacement);
	}

	[Fact]
	public void Slider_TickPlacement_roundtrip_Both()
	{
		var s = new WpfSlider { TickPlacement = WpfTickPlacement.Both };
		Assert.Equal(WpfTickPlacement.Both, s.TickPlacement);
	}

	[Fact]
	public void Slider_TickPlacement_roundtrip_TopLeft()
	{
		var s = new WpfSlider { TickPlacement = WpfTickPlacement.TopLeft };
		Assert.Equal(WpfTickPlacement.TopLeft, s.TickPlacement);
	}

	[Fact]
	public void Slider_IsSnapToTickEnabled_roundtrip()
	{
		var s = new WpfSlider { IsSnapToTickEnabled = true };
		Assert.True(s.IsSnapToTickEnabled);
	}

	[Fact]
	public void Slider_IsDirectionReversed_roundtrip()
	{
		var s = new WpfSlider { IsDirectionReversed = true };
		Assert.True(s.IsDirectionReversed);
	}

	// ── IsSnapToTickEnabled via Value coercion ────────────────────────────────

	[Fact]
	public void Slider_snap_enabled_coerces_Value_to_nearest_tick()
	{
		var s = new WpfSlider
		{
			Maximum = 10.0,
			TickFrequency = 1.0,
			IsSnapToTickEnabled = true,
		};
		s.Value = 2.7;
		Assert.Equal(3.0, s.Value);
	}

	[Fact]
	public void Slider_snap_disabled_does_not_coerce_Value()
	{
		var s = new WpfSlider
		{
			Maximum = 10.0,
			TickFrequency = 1.0,
			IsSnapToTickEnabled = false,
		};
		s.Value = 2.7;
		Assert.Equal(2.7, s.Value);
	}

	// ── Headless: set all before realize → no throw, persists ────────────────

	[Fact]
	public void Slider_all_props_before_realize_no_throw()
	{
		var s = new WpfSlider
		{
			Orientation = WpfOrientation.Vertical,
			SmallChange = 2.0,
			LargeChange = 20.0,
			TickFrequency = 5.0,
			TickPlacement = WpfTickPlacement.Both,
			IsSnapToTickEnabled = true,
			IsDirectionReversed = true,
			Maximum = 50.0,
			Value = 10.0,
		};
		Assert.Equal(WpfOrientation.Vertical, s.Orientation);
		Assert.Equal(2.0, s.SmallChange);
		Assert.Equal(20.0, s.LargeChange);
		Assert.Equal(5.0, s.TickFrequency);
		Assert.Equal(WpfTickPlacement.Both, s.TickPlacement);
		Assert.True(s.IsSnapToTickEnabled);
		Assert.True(s.IsDirectionReversed);
		Assert.Equal(10.0, s.Value);
	}
}
