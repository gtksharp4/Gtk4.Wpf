using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.VisualLayer;

class MouseProbe : Border
{
	public readonly List<string> Log;
	public MouseProbe(List<string> log) => Log = log;
	protected override void OnMouseEnter(MouseEventArgs e) { base.OnMouseEnter(e); Log.Add("enter"); }
	protected override void OnMouseLeave(MouseEventArgs e) => Log.Add("leave");
	protected override void OnMouseMove(MouseEventArgs e) => Log.Add("move");
	protected override void OnMouseDown(MouseButtonEventArgs e) => Log.Add("down");
	protected override void OnMouseUp(MouseButtonEventArgs e) => Log.Add("up");
	protected override void OnPreviewMouseDown(MouseButtonEventArgs e) => Log.Add("pdown");
	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) => Log.Add("ldown");
	protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) => Log.Add("lup");
	protected override void OnMouseRightButtonDown(MouseButtonEventArgs e) => Log.Add("rdown");
	protected override void OnMouseWheel(MouseWheelEventArgs e) => Log.Add("wheel");
	protected override void OnGotMouseCapture(MouseEventArgs e) => Log.Add("got");
	protected override void OnLostMouseCapture(MouseEventArgs e) => Log.Add("lost");
}

public class MouseLayerTests : WpfGtkTestBase
{
	[Fact]
	public void Virtual_hooks_run_before_instance_handlers_in_WPF_order() => Run(() =>
	{
		var log = new List<string>();
		var el = new MouseProbe(log);
		el.MouseDown += (s, e) => log.Add("inst-down");
		el.MouseLeftButtonDown += (s, e) => log.Add("inst-ldown");

		el.DispatchGtkButton(null, 1, new Point(1, 1), 1, pressed: true);

		Assert.Equal(new[] { "pdown", "down", "ldown", "inst-ldown", "inst-down" }, Order(log));
	});

	static string[] Order(List<string> log) => log.ToArray();

	[Fact]
	public void Right_button_and_release_reach_their_virtuals() => Run(() =>
	{
		var log = new List<string>();
		var el = new MouseProbe(log);
		el.DispatchGtkButton(null, 3, new Point(1, 1), 1, pressed: true);
		Assert.Contains("rdown", log);
		log.Clear();
		el.DispatchGtkButton(null, 1, new Point(1, 1), 1, pressed: false);
		Assert.Equal(new[] { "up", "lup" }, log.ToArray());
	});

	[Fact]
	public void Move_and_wheel_and_capture_virtuals_are_invoked() => Run(() =>
	{
		var log = new List<string>();
		var el = new MouseProbe(log);
		el.DispatchGtkMotion(null, new Point(2, 2));
		Assert.Contains("move", log);
		Assert.True(el.CaptureMouse());
		el.ReleaseMouseCapture();
		Assert.Equal(new[] { "got", "lost" }, log.FindAll(s => s == "got" || s == "lost").ToArray());
	});

	[Fact]
	public void A_handled_event_does_not_reach_the_virtual() => Run(() =>
	{
		var log = new List<string>();
		var outer = new MouseProbe(log);
		var inner = new Border();
		outer.Child = inner;
		inner.MouseDown += (s, e) => e.Handled = true;
		inner.DispatchGtkButton(null, 1, new Point(1, 1), 1, pressed: true);
		Assert.DoesNotContain("down", log);
	});

	[Fact]
	public void Button_events_carry_the_innermost_element_as_OriginalSource() => Run(() =>
	{
		var outer = new Border();
		var inner = new Border();
		outer.Child = inner;
		object? original = null, source = null;
		outer.MouseDown += (s, e) => { original = e.OriginalSource; source = e.Source; };

		inner.DispatchGtkButton(null, 1, new Point(1, 1), 1, pressed: true);

		Assert.Same(inner, original);
		Assert.Same(inner, source);
	});

	[Fact]
	public void Cracked_left_button_event_keeps_the_OriginalSource() => Run(() =>
	{
		var outer = new Border();
		var inner = new Border();
		outer.Child = inner;
		object? original = null;
		outer.MouseLeftButtonDown += (s, e) => original = e.OriginalSource;
		inner.DispatchGtkButton(null, 1, new Point(1, 1), 1, pressed: true);
		Assert.Same(inner, original);
	});

	[Fact]
	public void Captured_element_is_the_OriginalSource_of_retargeted_input() => Run(() =>
	{
		var a = new Border();
		var b = new Border();
		object? original = null;
		b.MouseDown += (s, e) => original = e.OriginalSource;
		Assert.True(b.CaptureMouse());
		try { a.DispatchGtkButton(null, 1, new Point(1, 1), 1, pressed: true); }
		finally { b.ReleaseMouseCapture(); }
		Assert.Same(b, original);
	});

	[Fact]
	public void SetCursor_is_false_without_an_element_under_the_pointer() => Run(() =>
	{
		var savedDirect = Mouse.DirectlyOver;
		var savedSource = Mouse._lastPositionSource;
		try
		{
			Mouse.DirectlyOver = null;
			Mouse._lastPositionSource = null;
			Assert.False(Mouse.SetCursor(Cursors.Hand));
		}
		finally
		{
			Mouse.DirectlyOver = savedDirect;
			Mouse._lastPositionSource = savedSource;
		}
	});

	[Fact]
	public void SetCursor_applies_to_the_element_widget_and_OverrideCursor_wins() => Run(() =>
	{
		var el = new Border();
		var savedDirect = Mouse.DirectlyOver;
		try
		{
			_ = el.Widget;
			Mouse.DirectlyOver = el;
			Assert.True(Mouse.SetCursor(Cursors.Hand));
			Assert.Equal(Cursors.Hand.GdkName, el.Widget.Cursor?.Name);
			Mouse.OverrideCursor = Cursors.Wait;
			Assert.True(Mouse.SetCursor(Cursors.Hand));
			Assert.Equal(Cursors.Wait.GdkName, el.Widget.Cursor?.Name);
		}
		finally
		{
			Mouse.OverrideCursor = null;
			Mouse.DirectlyOver = savedDirect;
		}
	});

	[Fact]
	public void Synchronize_is_a_safe_no_op() => Run(() => Mouse.Synchronize());
}
