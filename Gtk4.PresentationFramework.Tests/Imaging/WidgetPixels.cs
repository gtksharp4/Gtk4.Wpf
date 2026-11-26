using System;
using System.Runtime.InteropServices;

namespace Gtk4.PresentationFramework.Tests.Imaging;

/// <summary>Renders a mapped widget with GTK's snapshot path and reads its pixels (BGRA, premultiplied).</summary>
sealed class WidgetPixels
{
	delegate IntPtr PtrFn(IntPtr a);

	delegate IntPtr NewFn();

	delegate void PaintFn(IntPtr paintable, IntPtr snapshot, double w, double h);

	delegate IntPtr RenderFn(IntPtr renderer, IntPtr node, IntPtr viewport);

	delegate void DownloadFn(IntPtr texture, IntPtr data, UIntPtr stride);

	delegate void UnrefFn(IntPtr obj);

	WidgetPixels(byte[] data, int width, int height)
	{
		Data = data;
		Width = width;
		Height = height;
	}

	public byte[] Data { get; }

	public int Width { get; }

	public int Height { get; }

	public (byte B, byte G, byte R, byte A) At(int x, int y)
	{
		int o = (y * Width + x) * 4;
		return (Data[o], Data[o + 1], Data[o + 2], Data[o + 3]);
	}

	public int NonTransparent()
	{
		int n = 0;
		for (int o = 3; o < Data.Length; o += 4)
			if (Data[o] != 0)
				n++;
		return n;
	}

	public static WidgetPixels Capture(Gtk.Widget widget)
	{
		int width = widget.Width, height = widget.Height;
		if (!widget.IsMapped || width <= 0 || height <= 0)
			throw new InvalidOperationException($"Widget not mapped or empty ({width}x{height}).");
		IntPtr gtk = Load("gtk-4-1.dll", "libgtk-4.so.1", "libgtk-4.1.dylib");
		IntPtr gobject = Load("gobject-2.0-0.dll", "libgobject-2.0.so.0", "libgobject-2.0.0.dylib");
		IntPtr paintable = Fn<PtrFn>(gtk, "gtk_widget_paintable_new")(widget.Handle);
		IntPtr snapshot = Fn<NewFn>(gtk, "gtk_snapshot_new")();
		Fn<PaintFn>(gtk, "gdk_paintable_snapshot")(paintable, snapshot, width, height);
		IntPtr node = Fn<PtrFn>(gtk, "gtk_snapshot_free_to_node")(snapshot);
		IntPtr texture = IntPtr.Zero;
		try
		{
			if (node == IntPtr.Zero)
				return new WidgetPixels(new byte[width * height * 4], width, height);
			IntPtr native = Fn<PtrFn>(gtk, "gtk_widget_get_native")(widget.Handle);
			IntPtr renderer = Fn<PtrFn>(gtk, "gtk_native_get_renderer")(native);
			texture = Fn<RenderFn>(gtk, "gsk_renderer_render_texture")(renderer, node, IntPtr.Zero);
			var texture0 = new Gdk.Texture(texture);
			int w = texture0.Width, h = texture0.Height;
			var data = new byte[w * h * 4];
			var pin = GCHandle.Alloc(data, GCHandleType.Pinned);
			try { Fn<DownloadFn>(gtk, "gdk_texture_download")(texture, pin.AddrOfPinnedObject(), (UIntPtr)(w * 4)); }
			finally { pin.Free(); }
			return new WidgetPixels(data, w, h);
		}
		finally
		{
			var unref = Fn<UnrefFn>(gobject, "g_object_unref");
			if (paintable != IntPtr.Zero) unref(paintable);
			if (node != IntPtr.Zero) Fn<UnrefFn>(gtk, "gsk_render_node_unref")(node);
		}
	}

	static IntPtr Load(string windows, string linux, string mac)
	{
		string name = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? windows
			: RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? mac : linux;
		return NativeLibrary.Load(name);
	}

	static T Fn<T>(IntPtr library, string symbol) where T : Delegate =>
		Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library, symbol));
}
