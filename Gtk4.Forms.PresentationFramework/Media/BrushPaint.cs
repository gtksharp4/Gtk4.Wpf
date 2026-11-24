namespace System.Windows.Media;

/// <summary>
/// The snapshot path of native containers: tile-brush backgrounds and <see cref="UIElement.OpacityMask"/>.
/// Called from the <c>Backdrop*</c> widget subclasses before and after the stock snapshot.
/// </summary>
static class BrushPaint
{
	/// <summary>The element's tile-brush background, or null when it has none (solid and gradient stay on CSS).</summary>
	internal static TileBrush? TileBackground(UIElement element) =>
		(element switch
		{
			Controls.Panel p => p.Background,
			Controls.Border b => b.Background,
			_ => null,
		}) as TileBrush;

	/// <summary>Draws the element's background and children through <paramref name="stock"/>, masked if requested.</summary>
	internal static void Snapshot(Gtk.Widget widget, Gtk.Snapshot snapshot, Action<Gtk.Snapshot> stock)
	{
		var owner = UIElement.OwnerOf(widget);
		var background = owner == null ? null : TileBackground(owner);
		var mask = owner?.OpacityMask;
		int w = widget.Width, h = widget.Height;
		if ((background == null && mask == null) || w <= 0 || h <= 0)
		{
			stock(snapshot);
			return;
		}
		var rect = Graphene.Rect.Alloc().Init(0, 0, w, h);
		Paint(snapshot, rect, background, mask, w, h, stock);
	}

	static void Paint(Gtk.Snapshot snapshot, Graphene.Rect rect, Brush? background, Brush? mask,
		int w, int h, Action<Gtk.Snapshot> stock)
	{
		if (mask != null)
		{
			snapshot.PushMask(Gsk.MaskMode.Alpha);
			Fill(snapshot, rect, mask, w, h);
			snapshot.Pop();
		}
		if (background != null)
			Fill(snapshot, rect, background, w, h);
		stock(snapshot);
		if (mask != null)
			snapshot.Pop();
	}

	static void Fill(Gtk.Snapshot snapshot, Graphene.Rect rect, Brush brush, int w, int h)
	{
		using var cr = snapshot.AppendCairo(rect);
		brush.ApplyToCairo(cr, w, h);
		cr.Paint();
	}
}
