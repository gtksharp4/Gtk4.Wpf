using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Gtk4.Wpf.Testing;

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

	// INPUT is a type followed by a union; the union's alignment follows the pointer size, so the layout is
	// 40 bytes on x64 and 28 on x86 (a fixed offset of 8 would be wrong in a 32-bit process).
	[StructLayout(LayoutKind.Explicit)]
	struct InputUnion
	{
		[FieldOffset(0)] public MouseInputData mouse;
		[FieldOffset(0)] public KeyInputData key;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct InputRecord
	{
		public uint type;
		public InputUnion data;
	}

	internal static int InputRecordSize => Marshal.SizeOf<InputRecord>();

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
		if (SendInput((uint)inputs.Length, inputs, InputRecordSize) != inputs.Length)
			throw new InvalidOperationException("SendInput injected fewer events than requested (error "
				+ Marshal.GetLastWin32Error() + "); the desktop may be locked or the target elevated.");
	}

	static InputRecord Mouse(uint flags) =>
		new InputRecord
		{
			type = InputMouse,
			data = new InputUnion { mouse = new MouseInputData { dwFlags = flags } },
		};

	static InputRecord Char(char c, bool up) => new InputRecord
	{
		type = InputKeyboard,
		data = new InputUnion { key = new KeyInputData { wScan = c, dwFlags = KeyUnicode | (up ? KeyUp : 0) } },
	};
}
