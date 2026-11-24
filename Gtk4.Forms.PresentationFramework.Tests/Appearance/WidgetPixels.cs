using System;
using System.Runtime.InteropServices;

namespace Gtk4.Forms.PresentationFramework.Tests.Appearance;

/// <summary>
/// Renders a mapped widget through GTK's snapshot path (widget paintable, render node, renderer texture) and
/// reads pixels back with <c>gdk_texture_download</c>. GtkSharp4 binds none of it, so the C functions are called
/// directly. Pixels are premultiplied BGRA; widgets here are opaque, so premultiplication does not matter.
/// </summary>
sealed class WidgetPixels
{
	delegate IntPtr PtrFn(IntPtr a);

	delegate IntPtr NewFn();

	delegate void SnapFn(IntPtr paintable, IntPtr snapshot, double width, double height);

	delegate IntPtr RenderFn(IntPtr renderer, IntPtr node, IntPtr viewport);

	delegate void DownloadFn(IntPtr texture, byte[] data, UIntPtr stride);

	delegate int IntFn(IntPtr a);

	delegate void UnrefFn(IntPtr a);

	readonly byte[] _data;
	readonly int _stride;

	WidgetPixels(byte[] data, int width, int height)
	{
		_data = data;
		Width = width;
		Height = height;
		_stride = width * 4;
	}

	internal int Width { get; }

	internal int Height { get; }

	static IntPtr Load(string windows, string linux, string mac)
	{
		string name = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? windows
			: RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? mac : linux;
		return NativeLibrary.Load(name);
	}

	static T Fn<T>(IntPtr lib, string symbol) where T : Delegate =>
		Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(lib, symbol));

	internal static WidgetPixels Capture(Gtk.Widget widget)
	{
		var gtk = Load("gtk-4-1.dll", "libgtk-4.so.1", "libgtk-4.1.dylib");
		var gobject = Load("gobject-2.0-0.dll", "libgobject-2.0.so.0", "libgobject-2.0.0.dylib");
		int w = widget.Width, h = widget.Height;
		if (w <= 0 || h <= 0) throw new InvalidOperationException("The widget has no size.");
		IntPtr paintable = Fn<PtrFn>(gtk, "gtk_widget_paintable_new")(widget.Handle);
		IntPtr snapshot = Fn<NewFn>(gtk, "gtk_snapshot_new")();
		Fn<SnapFn>(gtk, "gdk_paintable_snapshot")(paintable, snapshot, w, h);
		IntPtr node = Fn<PtrFn>(gtk, "gtk_snapshot_free_to_node")(snapshot);
		IntPtr texture = IntPtr.Zero;
		try
		{
			IntPtr native = Fn<PtrFn>(gtk, "gtk_widget_get_native")(widget.Handle);
			IntPtr renderer = Fn<PtrFn>(gtk, "gtk_native_get_renderer")(native);
			texture = Fn<RenderFn>(gtk, "gsk_renderer_render_texture")(renderer, node, IntPtr.Zero);
			int tw = Fn<IntFn>(gtk, "gdk_texture_get_width")(texture);
			int th = Fn<IntFn>(gtk, "gdk_texture_get_height")(texture);
			var data = new byte[tw * th * 4];
			Fn<DownloadFn>(gtk, "gdk_texture_download")(texture, data, (UIntPtr)(uint)(tw * 4));
			return new WidgetPixels(data, tw, th);
		}
		finally
		{
			var unref = Fn<UnrefFn>(gobject, "g_object_unref");
			if (texture != IntPtr.Zero) unref(texture);
			unref(paintable);
			if (node != IntPtr.Zero) Fn<UnrefFn>(gtk, "gsk_render_node_unref")(node);
		}
	}

	internal (int R, int G, int B) At(int x, int y)
	{
		int i = y * _stride + x * 4;
		return (_data[i + 2], _data[i + 1], _data[i]);
	}

	internal (int R, int G, int B) Center => At(Width / 2, Height / 2);
}
