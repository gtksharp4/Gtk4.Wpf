using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Input;

/// <summary>Managed mouse capture: Mouse.Capture / CaptureMouse / re-targeting.</summary>
public class MouseCaptureTests : MouseStateTestBase
{
	static readonly Point P = new Point(1, 1);

	[Fact]
	public void CaptureMouse_sets_IsMouseCaptured_and_Mouse_Captured_and_raises_GotMouseCapture()
	{
		var el = new MouseTestElement("a");
		bool got = false;
		el.GotMouseCapture += (s, e) => got = true;

		Assert.True(el.CaptureMouse());

		Assert.True(el.IsMouseCaptured);
		Assert.Same(el, Mouse.Captured);
		Assert.True(got);
	}

	[Fact]
	public void Capturing_second_element_raises_Lost_on_first_then_Got_on_second()
	{
		var a = new MouseTestElement("a");
		var b = new MouseTestElement("b");
		a.CaptureMouse();
		var log = new List<string>();
		a.LostMouseCapture += (s, e) => log.Add("a:Lost");
		b.GotMouseCapture += (s, e) => log.Add("b:Got");

		Assert.True(Mouse.Capture(b));

		Assert.Equal(new[] { "a:Lost", "b:Got" }, log);
		Assert.False(a.IsMouseCaptured);
		Assert.True(b.IsMouseCaptured);
	}

	[Fact]
	public void IsMouseCapturedChanged_fires_on_capture_and_release()
	{
		var el = new MouseTestElement();
		var values = new List<object?>();
		el.IsMouseCapturedChanged += (s, e) => values.Add(e.NewValue);
		el.CaptureMouse();
		el.ReleaseMouseCapture();
		Assert.Equal(new List<object?> { true, false }, values);
	}

	[Fact]
	public void ReleaseMouseCapture_clears_Captured_and_raises_LostMouseCapture()
	{
		var el = new MouseTestElement();
		bool lost = false;
		el.LostMouseCapture += (s, e) => lost = true;
		el.CaptureMouse();
		el.ReleaseMouseCapture();
		Assert.Null(Mouse.Captured);
		Assert.False(el.IsMouseCaptured);
		Assert.True(lost);
	}

	[Fact]
	public void Mouse_Capture_null_releases()
	{
		var el = new MouseTestElement();
		el.CaptureMouse();
		Assert.True(Mouse.Capture(null));
		Assert.Null(Mouse.Captured);
	}

	[Fact]
	public void CaptureMode_None_releases()
	{
		var el = new MouseTestElement();
		el.CaptureMouse();
		Assert.True(Mouse.Capture(el, CaptureMode.None));
		Assert.Null(Mouse.Captured);
		Assert.False(el.IsMouseCaptured);
	}

	[Fact]
	public void IsMouseCapturedProperty_is_read_only_default_false()
	{
		Assert.True(UIElement.IsMouseCapturedProperty.IsReadOnly);
		Assert.Equal(false, UIElement.IsMouseCapturedProperty.DefaultMetadata.DefaultValue);
		Assert.Equal(RoutingStrategy.Bubble, UIElement.GotMouseCaptureEvent.RoutingStrategy);
		Assert.Equal(typeof(MouseEventHandler), UIElement.LostMouseCaptureEvent.HandlerType);
	}

	[Fact]
	public void MouseMove_while_captured_routes_to_captured_element_from_sibling()
	{
		var root = new MouseTestElement("root");
		var captor = new MouseTestElement("captor", root);
		var sibling = new MouseTestElement("sibling", root);
		var log = new List<string>();
		captor.MouseMove += (s, e) => log.Add("captor");
		sibling.MouseMove += (s, e) => log.Add("sibling");
		captor.CaptureMouse();

		sibling.RaiseMouseMove(P);

		Assert.Equal(new[] { "captor" }, log);
	}

	[Fact]
	public void MouseUp_while_captured_routes_to_captured_element()
	{
		var root = new MouseTestElement("root");
		var captor = new MouseTestElement("captor", root);
		var sibling = new MouseTestElement("sibling", root);
		object? upSource = null;
		root.MouseLeftButtonUp += (s, e) => { };
		captor.MouseLeftButtonUp += (s, e) => upSource = e.Source;
		captor.CaptureMouse();

		sibling.RaiseMouseButton(MouseButton.Left, P, 1, pressed: false);

		Assert.Same(captor, upSource);
	}

	[Fact]
	public void SubTree_capture_delivers_to_descendant_hit_and_outside_hits_to_captor()
	{
		var root = new MouseTestElement("root");
		var captor = new MouseTestElement("captor", root);
		var inner = new MouseTestElement("inner", captor);
		var outside = new MouseTestElement("outside", root);
		var log = new List<string>();
		captor.MouseMove += (s, e) => log.Add("captor<-" + Tag(e.Source!));
		Mouse.Capture(captor, CaptureMode.SubTree);

		inner.RaiseMouseMove(P);
		outside.RaiseMouseMove(P);

		Assert.Equal(new[] { "captor<-inner", "captor<-captor" }, log);
	}
}
