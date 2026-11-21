using System.Runtime.InteropServices;

namespace System.Windows.Automation.Peers;

// GtkSharp4 binds none of these: announcements, relation lists and relation reset.
static class GtkInterop
{
	internal static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

	internal static bool IsMac => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

	internal static void Announce(IntPtr accessible, string message, int priority)
	{
		var bytes = new byte[System.Text.Encoding.UTF8.GetByteCount(message) + 1];
		System.Text.Encoding.UTF8.GetBytes(message, 0, message.Length, bytes, 0);
		AnnounceBytes(accessible, bytes, priority);
	}

	static void AnnounceBytes(IntPtr accessible, byte[] message, int priority)
	{
		if (IsWindows) Win.Announce(accessible, message, priority);
		else if (IsMac) Mac.Announce(accessible, message, priority);
		else Linux.Announce(accessible, message, priority);
	}

	internal static void ResetRelation(IntPtr accessible, int relation)
	{
		if (IsWindows) Win.ResetRelation(accessible, relation);
		else if (IsMac) Mac.ResetRelation(accessible, relation);
		else Linux.ResetRelation(accessible, relation);
	}

	/// <summary>Sets a reference-list relation: builds the GtkAccessibleList value GTK expects.</summary>
	internal static void UpdateRelation(IntPtr accessible, int relation, IntPtr[] targets)
	{
		var value = Marshal.AllocHGlobal(64);
		try
		{
			for (int i = 0; i < 64; i += 8) Marshal.WriteInt64(value, i, 0);
			// The accessible list copies the GList, so the nodes are freed here.
			var glist = IntPtr.Zero;
			foreach (var t in targets)
				glist = IsWindows ? Win.ListAppend(glist, t)
					: IsMac ? Mac.ListAppend(glist, t) : Linux.ListAppend(glist, t);
			var list = IsWindows ? Win.NewListFromList(glist)
				: IsMac ? Mac.NewListFromList(glist) : Linux.NewListFromList(glist);
			if (IsWindows) Win.ListFree(glist);
			else if (IsMac) Mac.ListFree(glist);
			else Linux.ListFree(glist);
			var type = IsWindows ? Win.ListType() : IsMac ? Mac.ListType() : Linux.ListType();
			if (IsWindows) { Win.ValueInit(value, type); Win.ValueTakeBoxed(value, list); }
			else if (IsMac) { Mac.ValueInit(value, type); Mac.ValueTakeBoxed(value, list); }
			else { Linux.ValueInit(value, type); Linux.ValueTakeBoxed(value, list); }
			var relations = new[] { relation };
			if (IsWindows) Win.UpdateRelationValue(accessible, 1, relations, value);
			else if (IsMac) Mac.UpdateRelationValue(accessible, 1, relations, value);
			else Linux.UpdateRelationValue(accessible, 1, relations, value);
			if (IsWindows) Win.ValueUnset(value);
			else if (IsMac) Mac.ValueUnset(value);
			else Linux.ValueUnset(value);
		}
		finally { Marshal.FreeHGlobal(value); }
	}

	static class Win
	{
		const string Gtk = "gtk-4-1.dll", GObject = "gobject-2.0-0.dll", GLib = "glib-2.0-0.dll";

		[DllImport(Gtk, EntryPoint = "gtk_accessible_announce")]
		internal static extern void Announce(IntPtr a, byte[] m, int p);

		[DllImport(Gtk, EntryPoint = "gtk_accessible_reset_relation")]
		internal static extern void ResetRelation(IntPtr a, int r);

		[DllImport(Gtk, EntryPoint = "gtk_accessible_list_get_type")]
		internal static extern UIntPtr ListType();

		[DllImport(Gtk, EntryPoint = "gtk_accessible_list_new_from_list")]
		internal static extern IntPtr NewListFromList(IntPtr glist);

		[DllImport(GLib, EntryPoint = "g_list_append")]
		internal static extern IntPtr ListAppend(IntPtr list, IntPtr data);

		[DllImport(GLib, EntryPoint = "g_list_free")]
		internal static extern void ListFree(IntPtr list);

		[DllImport(Gtk, EntryPoint = "gtk_accessible_update_relation_value")]
		internal static extern void UpdateRelationValue(IntPtr a, int n, int[] relations, IntPtr values);

		[DllImport(GObject, EntryPoint = "g_value_init")]
		internal static extern IntPtr ValueInit(IntPtr value, UIntPtr type);

		[DllImport(GObject, EntryPoint = "g_value_take_boxed")]
		internal static extern void ValueTakeBoxed(IntPtr value, IntPtr boxed);

		[DllImport(GObject, EntryPoint = "g_value_unset")]
		internal static extern void ValueUnset(IntPtr value);
	}

	static class Linux
	{
		const string Gtk = "libgtk-4.so.1", GObject = "libgobject-2.0.so.0", GLib = "libglib-2.0.so.0";

		[DllImport(Gtk, EntryPoint = "gtk_accessible_announce")]
		internal static extern void Announce(IntPtr a, byte[] m, int p);

		[DllImport(Gtk, EntryPoint = "gtk_accessible_reset_relation")]
		internal static extern void ResetRelation(IntPtr a, int r);

		[DllImport(Gtk, EntryPoint = "gtk_accessible_list_get_type")]
		internal static extern UIntPtr ListType();

		[DllImport(Gtk, EntryPoint = "gtk_accessible_list_new_from_list")]
		internal static extern IntPtr NewListFromList(IntPtr glist);

		[DllImport(GLib, EntryPoint = "g_list_append")]
		internal static extern IntPtr ListAppend(IntPtr list, IntPtr data);

		[DllImport(GLib, EntryPoint = "g_list_free")]
		internal static extern void ListFree(IntPtr list);

		[DllImport(Gtk, EntryPoint = "gtk_accessible_update_relation_value")]
		internal static extern void UpdateRelationValue(IntPtr a, int n, int[] relations, IntPtr values);

		[DllImport(GObject, EntryPoint = "g_value_init")]
		internal static extern IntPtr ValueInit(IntPtr value, UIntPtr type);

		[DllImport(GObject, EntryPoint = "g_value_take_boxed")]
		internal static extern void ValueTakeBoxed(IntPtr value, IntPtr boxed);

		[DllImport(GObject, EntryPoint = "g_value_unset")]
		internal static extern void ValueUnset(IntPtr value);
	}

	static class Mac
	{
		const string Gtk = "libgtk-4.1.dylib", GObject = "libgobject-2.0.0.dylib", GLib = "libglib-2.0.0.dylib";

		[DllImport(Gtk, EntryPoint = "gtk_accessible_announce")]
		internal static extern void Announce(IntPtr a, byte[] m, int p);

		[DllImport(Gtk, EntryPoint = "gtk_accessible_reset_relation")]
		internal static extern void ResetRelation(IntPtr a, int r);

		[DllImport(Gtk, EntryPoint = "gtk_accessible_list_get_type")]
		internal static extern UIntPtr ListType();

		[DllImport(Gtk, EntryPoint = "gtk_accessible_list_new_from_list")]
		internal static extern IntPtr NewListFromList(IntPtr glist);

		[DllImport(GLib, EntryPoint = "g_list_append")]
		internal static extern IntPtr ListAppend(IntPtr list, IntPtr data);

		[DllImport(GLib, EntryPoint = "g_list_free")]
		internal static extern void ListFree(IntPtr list);

		[DllImport(Gtk, EntryPoint = "gtk_accessible_update_relation_value")]
		internal static extern void UpdateRelationValue(IntPtr a, int n, int[] relations, IntPtr values);

		[DllImport(GObject, EntryPoint = "g_value_init")]
		internal static extern IntPtr ValueInit(IntPtr value, UIntPtr type);

		[DllImport(GObject, EntryPoint = "g_value_take_boxed")]
		internal static extern void ValueTakeBoxed(IntPtr value, IntPtr boxed);

		[DllImport(GObject, EntryPoint = "g_value_unset")]
		internal static extern void ValueUnset(IntPtr value);
	}
}
