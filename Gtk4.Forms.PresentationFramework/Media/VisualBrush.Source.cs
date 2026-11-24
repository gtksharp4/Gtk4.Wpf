namespace System.Windows.Media;

public partial class VisualBrush
{
	void ApplyElement(Cairo.Context cr, double width, double height, UIElement element)
	{
		var surface = RenderSource(element);
		if (surface == null)
		{
			SetTransparentSource(cr);
			return;
		}
		var content = new Rect(0, 0, surface.Width, surface.Height);
		ApplyTile(cr, width, height, content, c =>
		{
			c.SetSourceSurface(surface, 0, 0);
			c.Paint();
		});
	}

	Cairo.ImageSurface? RenderSource(UIElement element)
	{
		var widget = element.Widget;
		var size = element.RenderSize;
		int w = (int)Math.Ceiling(size.Width), h = (int)Math.Ceiling(size.Height);
		if (w <= 0 || h <= 0) return null;
		using var paintable = new Gtk.WidgetPaintable(widget);
		using var snapshot = new Gtk.Snapshot();
		paintable.Snapshot(snapshot, w, h);
		using var node = snapshot.FreeToNode();
		if (node == null) return null;
		var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, w, h);
		using var cr = new Cairo.Context(surface);
		node.Draw(cr);
		return surface;
	}
}
