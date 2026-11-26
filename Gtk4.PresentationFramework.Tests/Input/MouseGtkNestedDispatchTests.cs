using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;
using WpfButton = System.Windows.Controls.Button;

namespace Gtk4.PresentationFramework.Tests.Input;

/// <summary>
/// Real-widget tests for nested GTK controllers: a Border hosting a Border, both with a
/// GestureClick (Button = 0), receive one GDK press in GTK's bubble order (inner first). The WPF
/// route must run once, even when the inner element captures inside MouseLeftButtonDown.
/// </summary>
public class MouseGtkNestedDispatchTests : WpfGtkTestBase
{
	static readonly MouseEventKey Press = new MouseEventKey((int)Gdk.EventType.ButtonPress, 4242, 12, 8);

	static int ClickControllers(Gtk.Widget widget)
	{
		int n = 0;
		var model = widget.ObserveControllers();
		for (uint i = 0; i < model.NItems; i++)
			if (model.GetObject(i) is Gtk.GestureClick gc && gc.Button == 0) n++;
		return n;
	}

	static void ResetMouse()
	{
		Mouse.Capture(null);
		MouseDispatchDedupe.Reset();
		Mouse.SetButtonState(MouseButton.Left, MouseButtonState.Released);
	}

	[Fact]
	public void Capture_inside_MouseLeftButtonDown_on_nested_widgets_delivers_one_press() => Run(() =>
	{
		ResetMouse();
		var inner = new Border { Width = 20, Height = 20 };
		var outer = new Border { Child = inner };
		Assert.Equal(1, ClickControllers(inner.Widget));
		Assert.Equal(1, ClickControllers(outer.Widget));
		var log = new List<string>();
		inner.MouseLeftButtonDown += (_, _) => { log.Add("inner:left"); inner.CaptureMouse(); };
		inner.MouseDown += (_, _) => log.Add("inner:down");
		outer.MouseDown += (_, _) => log.Add("outer:down");
		try
		{
			// GTK bubble order for one unclaimed press: the inner controller, then the outer one.
			inner.DispatchGtkButton(Press, 1, new Point(12, 8), 1, pressed: true);
			outer.DispatchGtkButton(Press, 1, new Point(14, 10), 1, pressed: true);

			Assert.Same(inner, Mouse.Captured);
			Assert.Equal(new[] { "inner:left", "inner:down", "outer:down" }, log);
		}
		finally { ResetMouse(); }
	});

	[Fact]
	public void Back_button_on_Win32_numbering_never_raises_a_left_click() => Run(() =>
	{
		ResetMouse();
		var btn = new WpfButton { Content = "x" };
		_ = btn.Widget;
		int left = 0;
		var buttons = new List<MouseButton>();
		btn.MouseLeftButtonDown += (_, _) => left++;
		btn.MouseDown += (_, e) => buttons.Add(e.ChangedButton);
		try
		{
			btn.DispatchGtkButton(Press, 4, new Point(1, 1), 1, pressed: true);
			btn.DispatchGtkButton(new MouseEventKey((int)Gdk.EventType.ButtonPress, 4243, 1, 1), 12,
				new Point(1, 1), 1, pressed: true);

			Assert.Equal(0, left);
			Assert.Equal(new[] { MouseButton.XButton1 }, buttons);
		}
		finally
		{
			Mouse.SetButtonState(MouseButton.XButton1, MouseButtonState.Released);
			ResetMouse();
		}
	});
}
