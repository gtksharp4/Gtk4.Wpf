namespace System.Windows.Media;

/// <summary>Marks the Backdrop* containers, which mask their own children (CSS background and border too).</summary>
interface IBackdrop { }

/// <summary>
/// The snapshot path of native containers: tile-brush backgrounds and <see cref="UIElement.OpacityMask"/>.
/// Called from the <c>Backdrop*</c> widget subclasses in place of the stock snapshot.
/// </summary>
/// <remarks>
/// GTK draws a widget's CSS background and border outside its own snapshot vfunc, so a mask can only cover them
/// from the parent: a Backdrop parent pushes each masked child's mask around <c>SnapshotChild</c>. A masked
/// element under any other parent (a window's content) masks its content only.
/// </remarks>
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

	/// <summary>Draws the element's background and children through <paramref name="stock"/>, masked if asked.</summary>
	internal static void Snapshot(Gtk.Widget widget, Gtk.Snapshot snapshot, Action<Gtk.Snapshot> stock)
	{
		var owner = UIElement.OwnerOf(widget);
		var background = owner == null ? null : TileBackground(owner);
		var mask = widget.Parent is IBackdrop ? null : owner?.OpacityMask;
		Action<Gtk.Snapshot> content = HasMaskedChild(widget) ? s => SnapshotChildren(widget, s) : stock;
		var size = GtkAllocationSync.BorderBoxSize(widget);
		int w = (int)Math.Ceiling(size.Width), h = (int)Math.Ceiling(size.Height);
		if ((background == null && mask == null) || w <= 0 || h <= 0)
		{
			content(snapshot);
			return;
		}
		if (mask != null)
		{
			snapshot.PushMask(Gsk.MaskMode.Alpha);
			Fill(snapshot, 0, 0, w, h, mask);
			snapshot.Pop();
		}
		if (background != null)
			Fill(snapshot, 0, 0, w, h, background);
		content(snapshot);
		if (mask != null)
			snapshot.Pop();
	}

	static bool HasMaskedChild(Gtk.Widget parent)
	{
		for (var c = parent.FirstChild; c != null; c = c.NextSibling)
			if (UIElement.OwnerOf(c)?.OpacityMask != null) return true;
		return false;
	}

	static void SnapshotChildren(Gtk.Widget parent, Gtk.Snapshot snapshot)
	{
		for (var c = parent.FirstChild; c != null; c = c.NextSibling)
		{
			var mask = UIElement.OwnerOf(c)?.OpacityMask;
			if (mask == null || !c.ComputeBounds(parent, out var b) || b.Width <= 0 || b.Height <= 0)
			{
				parent.SnapshotChild(c, snapshot);
				continue;
			}
			snapshot.PushMask(Gsk.MaskMode.Alpha);
			Fill(snapshot, b.X, b.Y, (int)Math.Ceiling(b.Width), (int)Math.Ceiling(b.Height), mask);
			snapshot.Pop();
			parent.SnapshotChild(c, snapshot);
			snapshot.Pop();
		}
	}

	internal static void Fill(Gtk.Snapshot snapshot, float x, float y, int w, int h, Brush brush)
	{
		using var rect = Graphene.Rect.Alloc();
		rect.Init(x, y, w, h);
		using var cr = snapshot.AppendCairo(rect);
		cr.Translate(x, y);
		brush.ApplyToCairo(cr, w, h);
		cr.Paint();
	}
}
