using System;
using System.Runtime.InteropServices;

/// <summary>
/// Keeps macOS from throttling a test run. Started from a terminal the process is never frontmost, so App Nap
/// may hold its GLib timers back: main-loop iterations blocked for up to 33 s on CI, even with no window shown.
/// An NSProcessInfo activity (user-initiated, latency-critical) is the documented opt-out. Shared with
/// Gtk4.Wpf.Testing.Tests as a linked file.
/// </summary>
static class MacActivity
{
	const string ObjC = "/usr/lib/libobjc.A.dylib";

	// NSActivityUserInitiated | NSActivityLatencyCritical
	const ulong Options = 0x00FFFFFFUL | 0xFF00000000UL;

	static IntPtr _activity;

	[DllImport(ObjC)]
	static extern IntPtr objc_getClass(string name);

	[DllImport(ObjC)]
	static extern IntPtr sel_registerName(string name);

	[DllImport(ObjC, EntryPoint = "objc_msgSend")]
	static extern IntPtr Send(IntPtr receiver, IntPtr selector);

	[DllImport(ObjC, EntryPoint = "objc_msgSend")]
	static extern IntPtr Send(IntPtr receiver, IntPtr selector, string utf8);

	[DllImport(ObjC, EntryPoint = "objc_msgSend")]
	static extern IntPtr Send(IntPtr receiver, IntPtr selector, ulong options, IntPtr reason);

	/// <summary>Begins the activity for the rest of the process; does nothing off macOS. Never throws.</summary>
	internal static void Begin()
	{
		if (!OperatingSystem.IsMacOS() || _activity != IntPtr.Zero) return;
		try
		{
			NativeLibrary.Load("/System/Library/Frameworks/Foundation.framework/Foundation");
			IntPtr info = Send(objc_getClass("NSProcessInfo"), sel_registerName("processInfo"));
			IntPtr reason = Send(Send(objc_getClass("NSString"), sel_registerName("alloc")),
				sel_registerName("initWithUTF8String:"), "GTK test run");
			IntPtr activity = info == IntPtr.Zero || reason == IntPtr.Zero ? IntPtr.Zero
				: Send(info, sel_registerName("beginActivityWithOptions:reason:"), Options, reason);
			if (activity == IntPtr.Zero)
			{
				Console.Error.WriteLine("MacRunner: could not begin an NSProcessInfo activity; App Nap stays on.");
				return;
			}
			_activity = Send(activity, sel_registerName("retain"));
			Console.Error.WriteLine("MacRunner: App Nap is off for this run (NSProcessInfo activity).");
		}
		catch (Exception e)
		{
			Console.Error.WriteLine("MacRunner: could not turn App Nap off: " + e.Message);
		}
	}
}
