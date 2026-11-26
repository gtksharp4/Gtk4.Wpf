using System;
using System.Collections.Generic;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>The capture-phase press controller behind Window.DragMove.</summary>
public class WindowDragMoveControllerTests : WpfGtkTestBase
{
	static Gtk.GestureClick? PressController(Window w)
	{
		var model = ((Gtk.Widget)w.Widget).ObserveControllers();
		for (uint i = 0; i < model.NItems; i++)
			if (model.GetObject(i) is Gtk.GestureClick c && c.PropagationPhase == Gtk.PropagationPhase.Capture)
				return c;
		return null;
	}

	[Fact]
	public void The_window_carries_a_capture_phase_click_controller() => Run(() =>
	{
		var w = new Window();
		w.Show();
		try { Assert.NotNull(PressController(w)); }
		finally { w.Hide(); }
	});

	[Fact]
	public void A_cancelled_gesture_clears_the_recorded_press() => Run(() =>
	{
		var w = new Window();
		w.Show();
		try
		{
			var click = PressController(w)!;
			Window.BeginMoveOverride = (_, _, _, _, _) => { };
			w.RecordPress(null, 1, 1, 1);
			GLib.Signal.Emit(click, "cancel", (Gdk.EventSequence?)null!);
			Assert.Throws<InvalidOperationException>(() => w.DragMove());
		}
		finally
		{
			Window.BeginMoveOverride = null;
			w.Hide();
		}
	});

	[Fact]
	public void A_press_whose_button_is_no_longer_down_is_stale() => Run(() =>
	{
		var w = new Window();
		w.Show();
		bool moved = false;
		try
		{
			var pointer = Gdk.Display.Default?.DefaultSeat?.Pointer;
			Assert.SkipUnless(pointer != null, "No pointer device.");
			Window.BeginMoveOverride = (_, _, _, _, _) => moved = true;

			Window.PrimaryDownOverride = _ => false;
			w.RecordPress(pointer, 1, 1, 1);
			Assert.Throws<InvalidOperationException>(() => w.DragMove());
			Assert.False(moved);

			Window.PrimaryDownOverride = _ => true;
			w.RecordPress(pointer, 1, 1, 1);
			w.DragMove();
			Assert.True(moved);

			// A backend that cannot report the button state (GDK Win32) gives the press the benefit of the doubt.
			moved = false;
			Window.PrimaryDownOverride = _ => null;
			w.RecordPress(pointer, 1, 1, 1);
			w.DragMove();
			Assert.True(moved);
		}
		finally
		{
			Window.BeginMoveOverride = null;
			Window.PrimaryDownOverride = null;
			w.Hide();
		}
	});
}
