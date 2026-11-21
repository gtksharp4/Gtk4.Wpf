using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Gtk4.Forms.Wpf.Testing;

public sealed partial class NativeInput
{
	const uint InputMouse = 0;
	const uint InputKeyboard = 1;
	const uint MouseLeftDown = 0x0002, MouseLeftUp = 0x0004, MouseRightDown = 0x0008, MouseRightUp = 0x0010;
	const uint KeyUnicode = 0x0004, KeyUp = 0x0002;
	[DllImport("user32.dll")]
	static extern bool GetClientRect(IntPtr hwnd, out NativeRect rect);

	[StructLayout(LayoutKind.Sequential)]
	struct NativeRect
	{
		public int left, top, right, bottom;
	}

	const string ToplevelClass = "gdkSurfaceToplevel";

	delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

	[StructLayout(LayoutKind.Sequential)]
	struct MouseInputData
	{
		public int dx, dy;
		public uint mouseData, dwFlags, time;
		public UIntPtr dwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct KeyInputData
	{
		public ushort wVk, wScan;
		public uint dwFlags, time;
		public UIntPtr dwExtraInfo;
	}

	// The union is sized by its largest member, MOUSEINPUT, which makes INPUT 40 bytes on x64.
	[StructLayout(LayoutKind.Explicit)]
	struct InputRecord
	{
		[FieldOffset(0)] public uint type;
		[FieldOffset(8)] public MouseInputData mouse;
		[FieldOffset(8)] public KeyInputData key;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct NativePoint
	{
		public int x, y;
	}

	[DllImport("user32.dll", SetLastError = true)]
	static extern uint SendInput(uint count, InputRecord[] inputs, int size);

	[DllImport("user32.dll")]
	static extern bool SetCursorPos(int x, int y);

	[DllImport("user32.dll")]
	static extern bool ClientToScreen(IntPtr hwnd, ref NativePoint point);

	[DllImport("user32.dll")]
	static extern bool SetForegroundWindow(IntPtr hwnd);

	[DllImport("user32.dll")]
	static extern bool IsWindowVisible(IntPtr hwnd);

	[DllImport("user32.dll")]
	static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);

	[DllImport("user32.dll")]
	static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint processId);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	static extern int GetClassName(IntPtr hwnd, StringBuilder text, int max);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int max);

	static IntPtr FindToplevel(string title)
	{
		IntPtr found = IntPtr.Zero;
		uint self = (uint)Environment.ProcessId;
		EnumWindows((hwnd, _) =>
		{
			GetWindowThreadProcessId(hwnd, out uint pid);
			if (pid != self || !IsWindowVisible(hwnd)) return true;
			var cls = new StringBuilder(64);
			GetClassName(hwnd, cls, cls.Capacity);
			var text = new StringBuilder(256);
			GetWindowText(hwnd, text, text.Capacity);
			if (cls.ToString() != ToplevelClass || text.ToString() != title) return true;
			found = hwnd;
			return false;
		}, IntPtr.Zero);
		return found;
	}

	static void Send(params InputRecord[] inputs)
	{
		if (SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<InputRecord>()) != inputs.Length)
			throw new InvalidOperationException("SendInput injected fewer events than requested (error "
				+ Marshal.GetLastWin32Error() + "); the desktop may be locked or the target elevated.");
	}

	static InputRecord Mouse(uint flags) =>
		new InputRecord { type = InputMouse, mouse = new MouseInputData { dwFlags = flags } };

	static InputRecord Char(char c, bool up) => new InputRecord
	{
		type = InputKeyboard,
		key = new KeyInputData { wScan = c, dwFlags = KeyUnicode | (up ? KeyUp : 0) },
	};
}
