using System.Runtime.InteropServices;

namespace System.Windows.Input;

// GtkSharp4 does not bind gdk_keyval_to_unicode.
static class GdkKeyText
{
	[DllImport("gtk-4-1.dll", EntryPoint = "gdk_keyval_to_unicode")]
	static extern uint ToUnicodeWindows(uint keyval);

	[DllImport("libgtk-4.so.1", EntryPoint = "gdk_keyval_to_unicode")]
	static extern uint ToUnicodeLinux(uint keyval);

	[DllImport("libgtk-4.1.dylib", EntryPoint = "gdk_keyval_to_unicode")]
	static extern uint ToUnicodeMac(uint keyval);

	/// <summary>The printable text of a GDK keyval (any layout), or null for control and non-text keys.</summary>
	internal static string? FromKeyval(uint keyval)
	{
		uint code = ToUnicode(keyval);
		if (code < 0x20 || (code >= 0x7f && code < 0xa0) || code > 0x10ffff || (code >= 0xd800 && code <= 0xdfff))
			return null;
		return char.ConvertFromUtf32((int)code);
	}

	static uint ToUnicode(uint keyval)
	{
		try
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return ToUnicodeWindows(keyval);
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return ToUnicodeMac(keyval);
			return ToUnicodeLinux(keyval);
		}
		catch (Exception e) when (e is DllNotFoundException || e is EntryPointNotFoundException)
		{
			return Latin1(keyval);
		}
	}

	// Without GDK only Latin-1 keysyms and directly encoded code points are known.
	static uint Latin1(uint keyval)
	{
		if (keyval >= 0x01000000) return keyval - 0x01000000;
		return keyval < 0x100 ? keyval : 0;
	}
}
