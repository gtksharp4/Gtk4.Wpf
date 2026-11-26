using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Input;

/// <summary><c>PreviewMouseMove</c> and <c>IsMouseCaptureWithin</c>.</summary>
public class MouseMoveCaptureWithinTests : MouseStateTestBase
{
	static readonly Point P = new Point(2, 2);

	[Fact]
	public void PreviewMouseMove_is_a_tunnel_MouseEventHandler_event_owned_by_UIElement()
	{
		Assert.Equal("PreviewMouseMove", UIElement.PreviewMouseMoveEvent.Name);
		Assert.Equal(RoutingStrategy.Tunnel, UIElement.PreviewMouseMoveEvent.RoutingStrategy);
		Assert.Equal(typeof(MouseEventHandler), UIElement.PreviewMouseMoveEvent.HandlerType);
	}

	[Fact]
	public void PreviewMouseMove_tunnels_root_first_before_MouseMove_bubbles()
	{
		var root = new MouseTestElement("root");
		var leaf = new MouseTestElement("leaf", root);
		var log = new List<string>();
		foreach (var el in new[] { root, leaf })
		{
			el.PreviewMouseMove += (s, _) => log.Add("preview:" + Tag(s));
			el.MouseMove += (s, _) => log.Add("move:" + Tag(s));
		}

		leaf.RaiseMouseMove(P);

		Assert.Equal(new[] { "preview:root", "preview:leaf", "move:leaf", "move:root" }, log);
	}

	[Fact]
	public void Handled_PreviewMouseMove_raises_MouseMove_already_handled()
	{
		var el = new MouseTestElement();
		int plain = 0, handledToo = 0;
		el.PreviewMouseMove += (_, e) => e.Handled = true;
		el.MouseMove += (_, _) => plain++;
		el.AddHandler(UIElement.MouseMoveEvent, new MouseEventHandler((_, _) => handledToo++), true);

		el.RaiseMouseMove(P);

		Assert.Equal(0, plain);
		Assert.Equal(1, handledToo);
	}

	[Fact]
	public void IsMouseCaptureWithin_is_set_on_the_captor_and_its_ancestors_only()
	{
		var root = new MouseTestElement("root");
		var mid = new MouseTestElement("mid", root);
		var leaf = new MouseTestElement("leaf", mid);
		var other = new MouseTestElement("other", root);

		leaf.CaptureMouse();

		Assert.True(leaf.IsMouseCaptureWithin);
		Assert.True(mid.IsMouseCaptureWithin);
		Assert.True(root.IsMouseCaptureWithin);
		Assert.False(other.IsMouseCaptureWithin);
		Assert.False(mid.IsMouseCaptured);
	}

	[Fact]
	public void Moving_capture_updates_only_the_elements_whose_value_changes()
	{
		var root = new MouseTestElement("root");
		var a = new MouseTestElement("a", root);
		var b = new MouseTestElement("b", root);
		var log = new List<string>();
		foreach (var el in new[] { root, a, b })
			el.IsMouseCaptureWithinChanged += (s, e) => log.Add(Tag(s) + "=" + e.NewValue);
		a.CaptureMouse();
		log.Clear();

		b.CaptureMouse();

		Assert.Equal(new[] { "a=False", "b=True" }, log); // root stays true: no change event
		Assert.True(root.IsMouseCaptureWithin);
	}

	[Fact]
	public void Releasing_capture_clears_IsMouseCaptureWithin()
	{
		var root = new MouseTestElement("root");
		var leaf = new MouseTestElement("leaf", root);
		leaf.CaptureMouse();

		leaf.ReleaseMouseCapture();

		Assert.False(leaf.IsMouseCaptureWithin);
		Assert.False(root.IsMouseCaptureWithin);
		Assert.Equal(false, UIElement.IsMouseCaptureWithinProperty.DefaultMetadata.DefaultValue);
		Assert.True(UIElement.IsMouseCaptureWithinProperty.IsReadOnly);
	}
}
