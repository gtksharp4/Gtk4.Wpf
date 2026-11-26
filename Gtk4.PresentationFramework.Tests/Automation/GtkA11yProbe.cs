using System;
using System.Runtime.InteropServices;

namespace Gtk4.PresentationFramework.Tests.Automation;

/// <summary>
/// Reads the GTK accessible tree through GTK's public test API (<c>gtk_test_accessible_*</c>), which
/// GtkSharp4 does not bind. The checks are variadic in C, so they are called with the argument shape
/// of each property: strings and integers pass as-is, doubles only where the variadic ABI allows.
/// </summary>
static class GtkA11yProbe
{
	delegate int HasFn(IntPtr accessible, int id);

	delegate IntPtr CheckIntFn(IntPtr accessible, int id, int value);

	delegate IntPtr CheckStrFn(IntPtr accessible, int id, IntPtr value);

	delegate IntPtr CheckBitsFn(IntPtr accessible, int id, long bits);

	[DllImport("kernel32", CharSet = CharSet.Unicode)]
	static extern IntPtr GetModuleHandleW(string name);

	static readonly Lazy<IntPtr> _lib = new Lazy<IntPtr>(Load);

	/// <summary>False where the variadic call cannot be made (macOS on ARM64 passes varargs on the stack).</summary>
	internal static bool CanCheckValues =>
		!(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
			&& RuntimeInformation.ProcessArchitecture == Architecture.Arm64);

	/// <summary>
	/// Doubles are variadic arguments: the SysV ABI wants AL set to the vector register count, which a
	/// non-variadic delegate never does, so only Windows (doubles also travel in integer registers) is reliable.
	/// </summary>
	internal static bool CanCheckDoubles => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

	static IntPtr Load()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return GetModuleHandleW("gtk-4-1.dll");
		foreach (var name in new[] { "libgtk-4.so.1", "libgtk-4.1.dylib", "libgtk-4.dylib" })
			if (NativeLibrary.TryLoad(name, out var handle))
				return handle;
		return IntPtr.Zero;
	}

	static T Fn<T>(string name) where T : Delegate
	{
		if (_lib.Value == IntPtr.Zero || !NativeLibrary.TryGetExport(_lib.Value, name, out var p))
			throw new InvalidOperationException("GTK test API not found: " + name);
		return Marshal.GetDelegateForFunctionPointer<T>(p);
	}

	internal static bool HasRole(Gtk.Widget w, Gtk.AccessibleRole role) =>
		Fn<HasFn>("gtk_test_accessible_has_role")(w.Handle, (int)role) != 0;

	internal static bool HasProperty(Gtk.Widget w, Gtk.AccessibleProperty p) =>
		Fn<HasFn>("gtk_test_accessible_has_property")(w.Handle, (int)p) != 0;

	internal static bool HasState(Gtk.Widget w, Gtk.AccessibleState s) =>
		Fn<HasFn>("gtk_test_accessible_has_state")(w.Handle, (int)s) != 0;

	// The test API returns NULL when the value matches, else a message that is not freed here.
	static bool Ok(IntPtr error) => error == IntPtr.Zero;

	internal static bool PropertyEquals(Gtk.Widget w, Gtk.AccessibleProperty p, string value)
	{
		var ptr = Marshal.StringToCoTaskMemUTF8(value);
		try { return Ok(Fn<CheckStrFn>("gtk_test_accessible_check_property")(w.Handle, (int)p, ptr)); }
		finally { Marshal.FreeCoTaskMem(ptr); }
	}

	internal static bool PropertyEquals(Gtk.Widget w, Gtk.AccessibleProperty p, bool value) =>
		Ok(Fn<CheckIntFn>("gtk_test_accessible_check_property")(w.Handle, (int)p, value ? 1 : 0));

	internal static bool PropertyEquals(Gtk.Widget w, Gtk.AccessibleProperty p, double value)
	{
		if (!CanCheckDoubles) throw new PlatformNotSupportedException("Check CanCheckDoubles first.");
		return Ok(Fn<CheckBitsFn>("gtk_test_accessible_check_property")(
			w.Handle, (int)p, BitConverter.DoubleToInt64Bits(value)));
	}

	internal static bool HasRelation(Gtk.Widget w, Gtk.AccessibleRelation r) =>
		Fn<HasFn>("gtk_test_accessible_has_relation")(w.Handle, (int)r) != 0;

	delegate IntPtr CheckRefFn(IntPtr accessible, int id, IntPtr target, IntPtr end);

	// Apple ARM64 passes variadic arguments on the stack. Padding the six remaining argument registers (x2-x7)
	// pushes the target and the NULL terminator onto the stack, where va_arg reads them.
	delegate IntPtr CheckRefStackFn(IntPtr accessible, int id, IntPtr x2, IntPtr x3, IntPtr x4, IntPtr x5,
		IntPtr x6, IntPtr x7, IntPtr target, IntPtr end);

	/// <summary>True when the relation holds exactly the single target (the variadic list ends with NULL).</summary>
	internal static bool RelationTargetEquals(Gtk.Widget w, Gtk.AccessibleRelation r, Gtk.Widget target)
	{
		const string name = "gtk_test_accessible_check_relation";
		if (CanCheckValues)
			return Ok(Fn<CheckRefFn>(name)(w.Handle, (int)r, target.Handle, IntPtr.Zero));
		var z = IntPtr.Zero;
		return Ok(Fn<CheckRefStackFn>(name)(w.Handle, (int)r, z, z, z, z, z, z, target.Handle, IntPtr.Zero));
	}

	internal static bool StateEquals(Gtk.Widget w, Gtk.AccessibleState s, int value) =>
		Ok(Fn<CheckIntFn>("gtk_test_accessible_check_state")(w.Handle, (int)s, value));
}
