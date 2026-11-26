using System;
using System.Runtime.InteropServices;

namespace Gtk4.Wpf.Testing;

/// <summary>
/// Renders a widget to PNG with GTK's own snapshot path (widget paintable, render node, renderer texture).
/// GtkSharp4 binds none of it, so the C functions are called directly.
/// </summary>
static class GtkSnapshot
{
	delegate IntPtr NewPaintableFn(IntPtr widget);

	delegate IntPtr NewSnapshotFn();

	delegate void PaintableSnapshotFn(IntPtr paintable, IntPtr snapshot, double width, double height);

	delegate IntPtr FreeToNodeFn(IntPtr snapshot);

	delegate IntPtr GetNativeFn(IntPtr widget);

	delegate IntPtr GetRendererFn(IntPtr native);

	delegate IntPtr RenderTextureFn(IntPtr renderer, IntPtr node, IntPtr viewport);

	delegate int SaveToPngFn(IntPtr texture, IntPtr filename);

	delegate void UnrefFn(IntPtr obj);

	/// <summary>Renders the mapped <paramref name="widget"/> and returns the PNG file bytes.</summary>
	internal static byte[] ToPng(Gtk.Widget widget)
	{
		GtkThread.AssertOnGtkThread();
		int width = widget.Width, height = widget.Height;
		if (!widget.IsMapped || width <= 0 || height <= 0)
			throw new InvalidOperationException("The widget is not mapped or has no size, so it cannot be rendered.");
		var gtk = GtkLibrary.Handle;
		IntPtr paintable = GtkLibrary.Fn<NewPaintableFn>(gtk, "gtk_widget_paintable_new")(widget.Handle);
		IntPtr snapshot = GtkLibrary.Fn<NewSnapshotFn>(gtk, "gtk_snapshot_new")();
		GtkLibrary.Fn<PaintableSnapshotFn>(gtk, "gdk_paintable_snapshot")(paintable, snapshot, width, height);
		IntPtr node = GtkLibrary.Fn<FreeToNodeFn>(gtk, "gtk_snapshot_free_to_node")(snapshot);
		IntPtr texture = IntPtr.Zero;
		string file = System.IO.Path.GetTempFileName();
		try
		{
			if (node == IntPtr.Zero)
				throw new InvalidOperationException("The widget produced nothing to render.");
			IntPtr native = GtkLibrary.Fn<GetNativeFn>(gtk, "gtk_widget_get_native")(widget.Handle);
			IntPtr renderer = GtkLibrary.Fn<GetRendererFn>(gtk, "gtk_native_get_renderer")(native);
			texture = GtkLibrary.Fn<RenderTextureFn>(gtk, "gsk_renderer_render_texture")(renderer, node, IntPtr.Zero);
			IntPtr name = Marshal.StringToCoTaskMemUTF8(file);
			try
			{
				var save = GtkLibrary.Fn<SaveToPngFn>(gtk, "gdk_texture_save_to_png");
				if (texture == IntPtr.Zero || save(texture, name) == 0)
					throw new InvalidOperationException("GTK could not render the widget to a PNG.");
			}
			finally { Marshal.FreeCoTaskMem(name); }
			return System.IO.File.ReadAllBytes(file);
		}
		finally
		{
			System.IO.File.Delete(file);
			var unref = GtkLibrary.Fn<UnrefFn>(GtkLibrary.GObject, "g_object_unref");
			if (texture != IntPtr.Zero) unref(texture);
			if (paintable != IntPtr.Zero) unref(paintable);
			if (node != IntPtr.Zero) GtkLibrary.Fn<UnrefFn>(gtk, "gsk_render_node_unref")(node);
		}
	}
}
