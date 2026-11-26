using System;
using System.Runtime.InteropServices;

namespace Gtk4.Wpf.Testing;

/// <summary>Resolves the native GTK and GObject libraries the process already loaded, per OS.</summary>
static class GtkLibrary
{
	static readonly Lazy<IntPtr> _gtk = new Lazy<IntPtr>(() =>
		Load("gtk-4-1.dll", "libgtk-4.so.1", "libgtk-4.1.dylib"));

	static readonly Lazy<IntPtr> _gobject = new Lazy<IntPtr>(() =>
		Load("gobject-2.0-0.dll", "libgobject-2.0.so.0", "libgobject-2.0.0.dylib"));

	internal static IntPtr Handle => _gtk.Value;

	internal static IntPtr GObject => _gobject.Value;

	static IntPtr Load(string windows, string linux, string mac)
	{
		string name = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? windows
			: RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? mac : linux;
		if (NativeLibrary.TryLoad(name, out var handle))
			return handle;
		throw new DllNotFoundException("The native GTK library " + name + " could not be loaded.");
	}

	internal static T Fn<T>(IntPtr library, string symbol) where T : Delegate
	{
		if (!NativeLibrary.TryGetExport(library, symbol, out var address))
			throw new EntryPointNotFoundException("GTK does not export " + symbol + ".");
		return Marshal.GetDelegateForFunctionPointer<T>(address);
	}
}
