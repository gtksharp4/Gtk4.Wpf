using System;
using System.Collections.Generic;
using System.Windows;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

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
}
