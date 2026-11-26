using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Input;

/// <summary>MouseWheelMap pure mapping and the PreviewMouseWheel/MouseWheel route.</summary>
public class MouseWheelTests : MouseStateTestBase
{
	[Theory]
	[InlineData(1.0, -120)]
	[InlineData(-1.0, 120)]
	[InlineData(0.5, -60)]
	[InlineData(0.0, 0)]
	[InlineData(2.0, -240)]
	public void ToDelta_maps_gtk_dy_to_wpf_delta(double dy, int expected) =>
		Assert.Equal(expected, MouseWheelMap.ToDelta(dy));

	[Fact]
	public void MouseWheelDeltaForOneLine_is_120() =>
		Assert.Equal(120, Mouse.MouseWheelDeltaForOneLine);

	[Fact]
	public void Wheel_events_have_WPF_strategies_and_handler_type()
	{
		Assert.Equal(RoutingStrategy.Tunnel, UIElement.PreviewMouseWheelEvent.RoutingStrategy);
		Assert.Equal(RoutingStrategy.Bubble, UIElement.MouseWheelEvent.RoutingStrategy);
		Assert.Equal(typeof(MouseWheelEventHandler), UIElement.MouseWheelEvent.HandlerType);
		Assert.Equal(typeof(UIElement), UIElement.MouseWheelEvent.OwnerType);
	}

	[Fact]
	public void PreviewMouseWheel_tunnels_before_MouseWheel_bubbles()
	{
		var parent = new MouseTestElement("parent");
		var child = new MouseTestElement("child", parent);
		var log = new List<string>();
		foreach (var el in new[] { parent, child })
		{
			el.PreviewMouseWheel += (s, e) => log.Add("P:" + Tag(s) + ":" + e.Delta);
			el.MouseWheel += (s, e) => log.Add("B:" + Tag(s) + ":" + e.Delta);
		}

		bool handled = child.RaiseMouseWheel(-120);

		Assert.False(handled);
		Assert.Equal(new[] { "P:parent:-120", "P:child:-120", "B:child:-120", "B:parent:-120" }, log);
	}

	[Fact]
	public void Handled_MouseWheel_returns_true_so_scroll_stops()
	{
		var parent = new MouseTestElement("parent");
		var child = new MouseTestElement("child", parent);
		parent.MouseWheel += (s, e) => e.Handled = true;
		Assert.True(child.RaiseMouseWheel(120));
	}

	[Fact]
	public void Handled_PreviewMouseWheel_skips_bubble_and_returns_true()
	{
		var el = new MouseTestElement("el");
		bool bubbled = false;
		el.PreviewMouseWheel += (s, e) => e.Handled = true;
		el.MouseWheel += (s, e) => bubbled = true;
		Assert.True(el.RaiseMouseWheel(120));
		Assert.False(bubbled);
	}
}
