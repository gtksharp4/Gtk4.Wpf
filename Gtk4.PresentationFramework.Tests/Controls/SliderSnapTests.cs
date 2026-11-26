using System.Collections.Generic;
using System.Linq;
using Xunit;
using SliderSnapHelper = System.Windows.Controls.SliderSnap;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure unit tests for the <c>SliderSnap</c> helpers (<c>SnapToTick</c> / <c>TickPositions</c>).
/// </summary>
public class SliderSnapTests
{
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
		Assert.Equal(new List<double> { 0, 3, 6, 9 }, ticks);
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
}
