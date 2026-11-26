using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Input;

/// <summary>Pure GDK → WPF mouse mappings: buttons, unit-aware wheel deltas, single dispatch.</summary>
public class MouseInputMapTests
{
	[Theory]
	[InlineData(1u, MouseButton.Left)]
	[InlineData(2u, MouseButton.Middle)]
	[InlineData(3u, MouseButton.Right)]
	[InlineData(4u, MouseButton.XButton1)] // Win32 XBUTTON1 / macOS buttonNumber+1
	[InlineData(5u, MouseButton.XButton2)]
	[InlineData(8u, MouseButton.XButton1)] // X11 / Wayland
	[InlineData(9u, MouseButton.XButton2)]
	public void Known_GDK_buttons_map_to_WPF_buttons(uint gdk, MouseButton expected)
	{
		Assert.True(MouseButtonMap.TryFromGdk(gdk, out var button));
		Assert.Equal(expected, button);
	}

	[Theory]
	[InlineData(0u)]
	[InlineData(6u)]
	[InlineData(7u)]
	[InlineData(10u)]
	[InlineData(15u)]
	public void Other_GDK_buttons_have_no_WPF_button(uint gdk)
	{
		Assert.False(MouseButtonMap.TryFromGdk(gdk, out _));
	}

	[Theory]
	[InlineData(1.0, false, -120)]
	[InlineData(-1.0, false, 120)]
	[InlineData(0.5, false, -60)]
	[InlineData(48.0, true, -120)]   // one notch of surface pixels
	[InlineData(-15.0, true, 38)]    // 15 px trackpad flick → 37.5 rounds to 38, not 1800
	[InlineData(0.1, true, 0)]       // sub-unit jitter is dropped by the caller
	public void Wheel_delta_is_in_wheel_units_for_both_GDK_scroll_units(double dy, bool surface, int expected)
	{
		Assert.Equal(expected, MouseWheelMap.ToDelta(dy, surface));
	}

	[Fact]
	public void Surface_pixels_per_notch_matches_WPF_ScrollViewer_three_lines_of_16px()
	{
		Assert.Equal(48.0, MouseWheelMap.SurfacePixelsPerNotch);
	}

	[Fact]
	public void Dedupe_skips_only_a_repeat_of_the_last_event()
	{
		MouseDispatchDedupe.Reset();
		var a = new MouseEventKey(4, 10, 1, 2);
		var b = new MouseEventKey(5, 10, 1, 2);

		Assert.False(MouseDispatchDedupe.IsDuplicate(a));
		Assert.True(MouseDispatchDedupe.IsDuplicate(a));
		Assert.False(MouseDispatchDedupe.IsDuplicate(b)); // different type
		Assert.False(MouseDispatchDedupe.IsDuplicate(a)); // a new delivery after another event
		MouseDispatchDedupe.Reset();
		Assert.False(MouseDispatchDedupe.IsDuplicate(a));
	}

	[Fact]
	public void Event_keys_compare_all_fields()
	{
		var k = new MouseEventKey(4, 10, 1, 2);
		Assert.Equal(k, new MouseEventKey(4, 10, 1, 2));
		Assert.Equal(k.GetHashCode(), new MouseEventKey(4, 10, 1, 2).GetHashCode());
		Assert.NotEqual(k, new MouseEventKey(4, 11, 1, 2));
		Assert.NotEqual(k, new MouseEventKey(4, 10, 1.5, 2));
		Assert.NotEqual(k, new MouseEventKey(4, 10, 1, 2.5));
	}
}
