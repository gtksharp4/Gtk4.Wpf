using System;

namespace System.Windows;

public partial class Window
{
	// The left press recorded by a capture-phase GestureClick on the window (cleared on release, cancel or stop).
	bool _leftDown;
	Gdk.Device? _pressDevice;
	double _pressX, _pressY;
	uint _pressTime;

	/// <summary>Replaces the native move so tests can observe its arguments (device, button, x, y, time).</summary>
	internal static Action<Gdk.Device?, int, double, double, uint>? BeginMoveOverride;

	void WireDragMove(Gtk.Window win)
	{
		var click = new Gtk.GestureClick { Button = 0, PropagationPhase = Gtk.PropagationPhase.Capture };
		click.Pressed += (o, a) =>
		{
			if (o is Gtk.GestureClick { CurrentButton: 1 } g)
			{
				RecordPress(g.CurrentEventDevice, a.X, a.Y, g.CurrentEventTime);
				OnShellPressed(a.X, a.Y);
			}
		};
		click.Released += (o, _) =>
		{
			if (o is Gtk.GestureClick { CurrentButton: 1 }) _leftDown = false;
		};
		click.Cancel += (_, _) => _leftDown = false;
		win.AddController(click);
	}

	/// <summary>Records a left-button press at window coordinates; converted to surface coordinates.</summary>
	internal void RecordPress(Gdk.Device? device, double x, double y, uint time)
	{
		double ox = 0, oy = 0;
		if (_window != null) _window.GetSurfaceTransform(out ox, out oy);
		_pressDevice = device;
		_pressX = x + ox;
		_pressY = y + oy;
		_pressTime = time;
		_leftDown = true;
	}

	/// <summary>
	/// Starts an interactive move of the window with the recorded left press. Throws
	/// <see cref="InvalidOperationException"/> unless the left button is down. GTK 4 hands the move to the
	/// compositor (<c>gdk_toplevel_begin_move</c>) and returns immediately instead of blocking until
	/// the button is released as WPF does; where the platform has no interactive move it does nothing.
	/// </summary>
	public void DragMove()
	{
		if (!_leftDown)
			throw new InvalidOperationException("Can only call DragMove when primary mouse button is down.");
		_leftDown = false;
		var surface = _window?.Surface;
		// A press whose button has since been released (e.g. the release went to another window) is stale.
		if (_pressDevice != null && IsPrimaryDown(surface, _pressDevice) == false)
			throw new InvalidOperationException("Can only call DragMove when primary mouse button is down.");
		if (BeginMoveOverride is { } hook)
		{
			hook(_pressDevice, 1, _pressX, _pressY, _pressTime);
			return;
		}
		if (surface == null) return;
		Gdk.ToplevelAdapter.GetObject(surface)?.BeginMove(_pressDevice!, 1, _pressX, _pressY, _pressTime);
	}

	/// <summary>Replaces the live pointer probe so tests can drive the stale-press check.</summary>
	internal static Func<Gdk.Device, bool?>? PrimaryDownOverride;

	// Null when the backend cannot say (GDK Win32 reports no device position).
	static bool? IsPrimaryDown(Gdk.Surface? surface, Gdk.Device device)
	{
		if (PrimaryDownOverride is { } probe) return probe(device);
		if (surface == null || !surface.GetDevicePosition(device, out _, out _, out var mask)) return null;
		return mask.HasFlag(Gdk.ModifierType.Button1Mask);
	}
}
