using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Input;

/// <summary>
/// The GTK-controller dispatch decision (<c>DispatchGtkButton</c>/<c>DispatchGtkMotion</c>):
/// one GDK event that GTK propagates through nested elements' controllers raises the WPF route
/// exactly once, and GDK buttons map onto WPF's five buttons only.
/// </summary>
public class MouseGtkDispatchTests : MouseStateTestBase
{
	static readonly Point P = new Point(3, 4);
	static readonly MouseEventKey Press = new MouseEventKey(4, 1000, 10, 20);
	static readonly MouseEventKey Release = new MouseEventKey(5, 1100, 10, 20);

	[Fact]
	public void Capture_taken_in_MouseLeftButtonDown_is_not_redelivered_by_the_ancestor_controller()
	{
		var parent = new MouseTestElement("parent");
		var child = new MouseTestElement("child", parent);
		int childDowns = 0, childLeftDowns = 0;
		child.MouseLeftButtonDown += (_, _) => { childLeftDowns++; child.CaptureMouse(); };
		child.MouseDown += (_, _) => childDowns++;

		// GTK order: innermost controller first, then the ancestor's with the same GDK event.
		Assert.True(child.DispatchGtkButton(Press, 1, P, 1, pressed: true));
		Assert.False(parent.DispatchGtkButton(Press, 1, P, 1, pressed: true));

		Assert.Same(child, Mouse.Captured);
		Assert.Equal(1, childLeftDowns);
		Assert.Equal(1, childDowns);
	}

	[Fact]
	public void Release_after_ReleaseMouseCapture_in_MouseUp_is_not_redelivered()
	{
		var parent = new MouseTestElement("parent");
		var child = new MouseTestElement("child", parent);
		child.CaptureMouse();
		int parentUps = 0;
		parent.MouseUp += (_, _) => parentUps++;
		child.MouseUp += (_, _) => child.ReleaseMouseCapture();

		child.DispatchGtkButton(Release, 1, P, 1, pressed: false);
		parent.DispatchGtkButton(Release, 1, P, 1, pressed: false);

		Assert.Null(Mouse.Captured);
		Assert.Equal(1, parentUps); // the bubble from the child only
	}

	[Fact]
	public void Without_capture_an_ancestor_gets_MouseDown_and_MouseMove_once()
	{
		var parent = new MouseTestElement("parent");
		var child = new MouseTestElement("child", parent);
		int downs = 0, moves = 0;
		parent.MouseDown += (_, _) => downs++;
		parent.MouseMove += (_, _) => moves++;
		var motion = new MouseEventKey(3, 1200, 11, 21);

		child.DispatchGtkButton(Press, 1, P, 1, pressed: true);
		parent.DispatchGtkButton(Press, 1, P, 1, pressed: true);
		child.DispatchGtkMotion(motion, P);
		parent.DispatchGtkMotion(motion, P);

		Assert.Equal(1, downs);
		Assert.Equal(1, moves);
	}

	[Fact]
	public void Distinct_GDK_events_are_each_dispatched()
	{
		var el = new MouseTestElement();
		int downs = 0;
		el.MouseDown += (_, _) => downs++;

		el.DispatchGtkButton(Press, 1, P, 1, pressed: true);
		el.DispatchGtkButton(new MouseEventKey(4, 1300, 10, 20), 1, P, 2, pressed: true);

		Assert.Equal(2, downs);
	}

	[Fact]
	public void Unknown_GDK_button_raises_nothing()
	{
		var el = new MouseTestElement();
		var log = new List<string>();
		el.MouseDown += (_, e) => log.Add("down:" + e.ChangedButton);
		el.MouseLeftButtonDown += (_, _) => log.Add("left");

		Assert.False(el.DispatchGtkButton(Press, 10, P, 1, pressed: true));

		Assert.Empty(log);
		Assert.Equal(MouseButtonState.Released, Mouse.LeftButton);
	}

	[Theory]
	[InlineData(4u, MouseButton.XButton1)]
	[InlineData(5u, MouseButton.XButton2)]
	public void Win32_and_macOS_extended_buttons_are_X_buttons_not_left(uint gdk, MouseButton expected)
	{
		var el = new MouseTestElement();
		MouseButton? changed = null;
		int left = 0;
		el.MouseDown += (_, e) => changed = e.ChangedButton;
		el.MouseLeftButtonDown += (_, _) => left++;

		el.DispatchGtkButton(Press, gdk, P, 1, pressed: true);

		Assert.Equal(expected, changed);
		Assert.Equal(0, left);
		Assert.Equal(MouseButtonState.Released, Mouse.LeftButton);
	}
}
