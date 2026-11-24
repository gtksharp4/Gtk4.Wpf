using System.Windows.Media;

namespace System.Windows.Controls;

// Stock GTK containers that draw a tile-brush background and an OpacityMask around their normal snapshot.
// Each keeps its parent's CSS node name. A GtkSharp subclass reports a __gtksharp_* widget name, which
// ElementCssStyle would take for a user-given one (and its #name selector would never match), so each
// constructor restores a Gtk* default name.

sealed class BackdropBox : Gtk.Box
{
	internal BackdropBox(Gtk.Orientation orientation, int spacing) : base(orientation, spacing) { Name = "GtkBackdropBox"; }

	protected override void OnSnapshot(Gtk.Snapshot snapshot) => BrushPaint.Snapshot(this, snapshot, base.OnSnapshot);
}

sealed class BackdropFixed : Gtk.Fixed
{
	internal BackdropFixed() { Name = "GtkBackdropFixed"; }

	protected override void OnSnapshot(Gtk.Snapshot snapshot) => BrushPaint.Snapshot(this, snapshot, base.OnSnapshot);
}

sealed class BackdropGrid : Gtk.Grid
{
	internal BackdropGrid() { Name = "GtkBackdropGrid"; }

	protected override void OnSnapshot(Gtk.Snapshot snapshot) => BrushPaint.Snapshot(this, snapshot, base.OnSnapshot);
}

sealed class BackdropFrame : Gtk.Frame
{
	internal BackdropFrame() { Name = "GtkBackdropFrame"; }

	protected override void OnSnapshot(Gtk.Snapshot snapshot) => BrushPaint.Snapshot(this, snapshot, base.OnSnapshot);
}
