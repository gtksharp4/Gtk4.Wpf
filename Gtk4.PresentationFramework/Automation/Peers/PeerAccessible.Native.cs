using System.Runtime.InteropServices;

namespace System.Windows.Automation.Peers;

// GtkSharp4 binds UpdateProperty but not gtk_accessible_reset_property.
static partial class PeerAccessible
{
	[DllImport("gtk-4-1.dll", EntryPoint = "gtk_accessible_reset_property")]
	static extern void ResetWindows(IntPtr accessible, int property);

	[DllImport("libgtk-4.so.1", EntryPoint = "gtk_accessible_reset_property")]
	static extern void ResetLinux(IntPtr accessible, int property);

	[DllImport("libgtk-4.1.dylib", EntryPoint = "gtk_accessible_reset_property")]
	static extern void ResetMac(IntPtr accessible, int property);

	/// <summary>Removes an explicit property so GTK derives it from the widget again.</summary>
	internal static void ResetProperty(Gtk.Widget widget, Gtk.AccessibleProperty property)
	{
		Spy?.Invoke(widget, property, null);
		try
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ResetWindows(widget.Handle, (int)property);
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) ResetMac(widget.Handle, (int)property);
			else ResetLinux(widget.Handle, (int)property);
		}
		catch (Exception e) when (e is DllNotFoundException || e is EntryPointNotFoundException)
		{
			// Without the symbol the stale value stays; nothing else can clear it.
		}
	}

	/// <summary>Sets a text property, or resets it when <paramref name="value"/> is empty.</summary>
	internal static void SetOrResetProperty(Gtk.Widget widget, Gtk.AccessibleProperty property, string? value)
	{
		if (string.IsNullOrEmpty(value)) ResetProperty(widget, property);
		else SetProperty(widget, property, value!);
	}
}
