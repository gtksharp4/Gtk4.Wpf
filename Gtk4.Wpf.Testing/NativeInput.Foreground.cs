using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;

namespace Gtk4.Wpf.Testing;

public sealed partial class NativeInput
{
	[DllImport("user32.dll")]
	static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll")]
	static extern IntPtr WindowFromPoint(NativePoint point);

	[DllImport("user32.dll")]
	static extern IntPtr GetAncestor(IntPtr hwnd, uint flags);

	const uint GaRoot = 2;

	// Windows' foreground-lock rules can refuse SetForegroundWindow silently, and injected input goes to whichever
	// window has focus, so never send until the app's window is confirmed in front.
	static void Activate(IntPtr hwnd)
	{
		for (int attempt = 0; attempt < 10; attempt++)
		{
			SetForegroundWindow(hwnd);
			if (GetForegroundWindow() == hwnd) return;
			Thread.Sleep(50);
		}
		throw new InvalidOperationException(
			"The window could not be brought to the foreground (Windows refused, or the desktop is locked).");
	}

	static void RequireTopmostAt(IntPtr hwnd, Point screen)
	{
		var at = new NativePoint { x = (int)Math.Round(screen.X), y = (int)Math.Round(screen.Y) };
		IntPtr hit = WindowFromPoint(at);
		if (hit == IntPtr.Zero || GetAncestor(hit, GaRoot) != hwnd)
			throw new InvalidOperationException("Another window covers the target point " + at.x + "," + at.y + ".");
	}

	// Bounds are relative to the element's own root, so only elements under the main window can be placed.
	void RequireMainWindow(AutomationElement element)
	{
		bool inMain = _host.Invoke(() =>
		{
			AutomationPeer? peer = element.Peer;
			while (peer != null && peer is not UIElementAutomationPeer) peer = peer.GetParent();
			var widget = (peer as UIElementAutomationPeer)?.Owner.Widget;
			return widget != null && widget.Root == _host.Window.Widget;
		});
		if (!inMain)
			throw new NotSupportedException(
				"Native input targets elements of the main window only (not other windows, dialogs or popups).");
	}
}
