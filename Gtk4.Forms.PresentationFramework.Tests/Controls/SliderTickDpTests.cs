using System.Collections.Generic;
using System.Linq;
using Xunit;
using WpfSlider       = System.Windows.Controls.Slider;
using WpfOrientation  = System.Windows.Controls.Orientation;
using WpfTickPlacement = System.Windows.Controls.Primitives.TickPlacement;
using SliderSnapHelper = System.Windows.Controls.SliderSnap;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure (no GTK) DP defaults, round-trips, snap, and tick-position tests.
/// </summary>
public class SliderTickDpTests
{
	// ── DP defaults ──────────────────────────────────────────────────────────

	[Fact]
	public void Slider_SmallChange_default_is_1()
	{
		Assert.Equal(1.0, new WpfSlider().SmallChange);
	}

	[Fact]
	public void Slider_LargeChange_default_is_10()
	{
		Assert.Equal(10.0, new WpfSlider().LargeChange);
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

	// ── SliderSnap.SnapToTick ─────────────────────────────────────────────────

	[Fact]
	public void SnapToTick_0_3_snaps_to_0()
	{
		Assert.Equal(0.0, SliderSnapHelper.SnapToTick(0.3, 0, 10, 1));
	}

	[Fact]
	public void SnapToTick_0_7_snaps_to_1()
	{
		Assert.Equal(1.0, SliderSnapHelper.SnapToTick(0.7, 0, 10, 1));
	}

	[Fact]
	public void SnapToTick_0_5_snaps_to_1_round_half_away_from_zero()
	{
		// Round-half-away-from-zero: 0.5 → 1
		Assert.Equal(1.0, SliderSnapHelper.SnapToTick(0.5, 0, 10, 1));
	}

	[Fact]
	public void SnapToTick_2_5_snaps_to_3_round_half_away_from_zero()
	{
		// Round-half-away-from-zero: 2.5 → 3
		Assert.Equal(3.0, SliderSnapHelper.SnapToTick(2.5, 0, 10, 1));
	}

	[Fact]
	public void SnapToTick_clamps_below_min()
	{
		Assert.Equal(0.0, SliderSnapHelper.SnapToTick(-5.0, 0, 10, 1));
	}

	[Fact]
	public void SnapToTick_clamps_above_max()
	{
		Assert.Equal(10.0, SliderSnapHelper.SnapToTick(15.0, 0, 10, 1));
	}

	[Fact]
	public void SnapToTick_frequency_zero_returns_value_unchanged()
	{
		Assert.Equal(3.7, SliderSnapHelper.SnapToTick(3.7, 0, 10, 0));
	}

	[Fact]
	public void SnapToTick_negative_frequency_returns_value_unchanged()
	{
		Assert.Equal(3.7, SliderSnapHelper.SnapToTick(3.7, 0, 10, -1));
	}

	[Fact]
	public void SnapToTick_fractional_frequency()
	{
		// 0.3 with freq=0.25 → ticks at 0, 0.25, 0.5 → nearest is 0.25
		Assert.Equal(0.25, SliderSnapHelper.SnapToTick(0.3, 0, 1, 0.25), 10);
	}

	// ── SliderSnap.TickPositions ──────────────────────────────────────────────

	[Fact]
	public void TickPositions_0_to_10_step_2_returns_expected()
	{
		var ticks = SliderSnapHelper.TickPositions(0, 10, 2).ToList();
		Assert.Equal(new List<double> { 0, 2, 4, 6, 8, 10 }, ticks);
	}

	[Fact]
	public void TickPositions_stops_at_max_boundary()
	{
		// 0..10 step 3 → 0,3,6,9 (10 is not a multiple of 3; not included)
		var ticks = SliderSnapHelper.TickPositions(0, 10, 3).ToList();
		Assert.Equal(new System.Collections.Generic.List<double> { 0, 3, 6, 9 }, ticks);
	}

	[Fact]
	public void TickPositions_zero_frequency_returns_empty()
	{
		Assert.Empty(SliderSnapHelper.TickPositions(0, 10, 0));
	}

	[Fact]
	public void TickPositions_negative_frequency_returns_empty()
	{
		Assert.Empty(SliderSnapHelper.TickPositions(0, 10, -1));
	}

	[Fact]
	public void TickPositions_empty_range_returns_empty()
	{
		Assert.Empty(SliderSnapHelper.TickPositions(5, 5, 1));
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
