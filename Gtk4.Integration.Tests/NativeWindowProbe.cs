using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;

namespace Gtk4.Integration.Tests;

/// <summary>
/// Windows-only helper that waits for a process to show a visible top-level window with a
/// given title (the backing GTK window's HWND still carries the title set via WPF).
/// </summary>
[SupportedOSPlatform("windows")]
static class NativeWindowProbe
{
	/// <summary>
	/// Polls until <paramref name="process"/> owns a visible window titled <paramref name="title"/>,
	/// the process exits, or <paramref name="timeout"/> elapses.
	/// </summary>
	public static bool WaitForWindow(Process process, string title, TimeSpan timeout)
	{
		DateTime deadline = DateTime.UtcNow + timeout;
		while (DateTime.UtcNow < deadline)
		{
			if (process.HasExited) return false;
			if (Find((uint)process.Id, title) != IntPtr.Zero) return true;
			Thread.Sleep(200);
		}
		return false;
	}

	static IntPtr Find(uint pid, string title)
	{
		IntPtr found = IntPtr.Zero;
		EnumWindows((hWnd, _) =>
		{
			GetWindowThreadProcessId(hWnd, out uint windowPid);
			if (windowPid == pid && IsWindowVisible(hWnd) && TitleOf(hWnd) == title)
			{
				found = hWnd;
				return false; // stop enumerating
			}
			return true;
		}, IntPtr.Zero);
		return found;
	}

	static string TitleOf(IntPtr hWnd)
	{
		int length = GetWindowTextLength(hWnd);
		if (length == 0) return string.Empty;
		var sb = new StringBuilder(length + 1);
		GetWindowText(hWnd, sb, sb.Capacity);
		return sb.ToString();
	}

	delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

	[DllImport("user32.dll")] static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);
	[DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
	[DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr hWnd);
	[DllImport("user32.dll")] static extern int GetWindowTextLength(IntPtr hWnd);
	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);
}