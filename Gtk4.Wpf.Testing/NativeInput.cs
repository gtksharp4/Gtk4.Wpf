using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;

namespace Gtk4.Wpf.Testing;

/// <summary>
/// Opt-in operating-system input (Windows <c>SendInput</c>): a real cursor move, click or keystroke that goes
/// through the window system and GTK's event controllers, not the layer's routed-input seam. It moves the user's
/// cursor and needs an interactive, unlocked desktop, so tests using it are not part of the CI gate (see
/// <see cref="WpfNativeFactAttribute"/>). Each action waits for the app to be idle, but the OS delivers input
/// asynchronously, so wait for the expected effect. Other platforms throw <see cref="PlatformNotSupportedException"/>.
/// </summary>
public sealed partial class NativeInput
{
	readonly WpfAppHost _host;

	internal NativeInput(WpfAppHost host) => _host = host;

	/// <summary>True where native injection is implemented (Windows).</summary>
	public static bool IsSupported => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

	/// <summary>
	/// Moves the cursor to the element's centre, or to <paramref name="at"/> in its own coordinates. Brings the window
	/// to the foreground first and throws <see cref="InvalidOperationException"/> if it cannot, or if another window
	/// covers the point; throws <see cref="NotSupportedException"/> for elements outside the main window.
	/// </summary>
	public void MoveTo(AutomationElement element, Point? at = null)
	{
		RequireSupported();
		RequireMainWindow(element);
		IntPtr hwnd = Handle();
		Activate(hwnd);
		Point target = Target(element, at, hwnd);
		MoveCursor(target);
		RequireTopmostAt(hwnd, target);
	}

	/// <summary>Moves the cursor onto the element and presses and releases the button.</summary>
	public void Click(AutomationElement element, MouseButton button = MouseButton.Left, Point? at = null)
	{
		MoveTo(element, at);
		Settle();
		bool right = button == MouseButton.Right;
		Send(Mouse(right ? MouseRightDown : MouseLeftDown), Mouse(right ? MouseRightUp : MouseLeftUp));
		Settle();
	}

	/// <summary>Types <paramref name="text"/> as Unicode keystrokes into the focused control of the window.</summary>
	public void TypeText(string text)
	{
		if (text == null) throw new ArgumentNullException(nameof(text));
		RequireSupported();
		Activate(Handle());
		foreach (char c in text)
			Send(Char(c, up: false), Char(c, up: true));
		Settle();
	}

	void MoveCursor(Point screen)
	{
		RequireSupported();
		SetCursorPos((int)Math.Round(screen.X), (int)Math.Round(screen.Y));
	}

	/// <summary>
	/// Screen position of a point in the element. GDK's Win32 window is larger than the GTK surface: it adds an
	/// invisible frame (measured 14 px per side, one more at the top). The frame width is the horizontal excess
	/// over the GTK window's size, halved; the top gets the vertical excess minus that.
	/// </summary>
	Point Target(AutomationElement element, Point? at, IntPtr hwnd)
	{
		RequireSupported();
		Rect bounds = element.Current.BoundingRectangle;
		if (bounds.IsEmpty)
			throw new InvalidOperationException("The element has no bounds, so it cannot be targeted.");
		var origin = new NativePoint();
		ClientToScreen(hwnd, ref origin);
		GetClientRect(hwnd, out NativeRect client);
		var (width, height, scale) = _host.Invoke(() =>
			(_host.Window.Widget.Width, _host.Window.Widget.Height, _host.Window.Widget.ScaleFactor));
		double frameX = Math.Max(0, (client.right - client.left - width * scale) / 2);
		double frameTop = Math.Max(0, client.bottom - client.top - height * scale - frameX);
		var offset = at ?? new Point(bounds.Width / 2, bounds.Height / 2);
		return new Point(origin.x + frameX + (bounds.X + offset.X) * scale,
			origin.y + frameTop + (bounds.Y + offset.Y) * scale);
	}

	IntPtr Handle()
	{
		string title = _host.Invoke(() => _host.Window.Title);
		IntPtr hwnd = FindToplevel(title);
		return hwnd != IntPtr.Zero ? hwnd
			: throw new InvalidOperationException("No visible top-level window titled '" + title + "' was found.");
	}

	// The OS queues input asynchronously: this drains what GTK already has, it does not prove delivery.
	void Settle() => _host.WaitForIdle();

	static void RequireSupported()
	{
		if (!IsSupported)
			throw new PlatformNotSupportedException("Native input injection is implemented on Windows only.");
	}
}
