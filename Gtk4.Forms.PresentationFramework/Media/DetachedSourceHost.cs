using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Media;

/// <summary>One brush's hold on a hosted source; the host releases it when the brush goes away.</summary>
sealed class HostLease
{
	internal HostLease(VisualBrush owner, Gtk.Widget widget, SourceLink? link)
	{
		Owner = new WeakReference<VisualBrush>(owner);
		Widget = widget;
		Link = link;
	}

	internal WeakReference<VisualBrush> Owner { get; }
	internal Gtk.Widget Widget { get; }
	internal SourceLink? Link { get; }
	internal bool Alive { get; set; } = true;
	internal bool MovePending { get; set; }
	internal bool SelfMapped { get; set; }
	internal bool SeenMapped { get; set; }
	internal Gtk.Fixed? Layer { get; set; }
}

/// <summary>
/// Parks unparented brush sources where GTK maps and draws them (it draws nothing for an unmapped widget).
/// A source is first mapped by hand, so it renders at once even with no window (RenderTargetBitmap-style
/// painting), then moved from an idle callback into an invisible layer of the window that paints the brush.
/// The layer is a non-targetable, non-focusable overlay child holding its sources far outside the window
/// surface, so there is no extra toplevel and nothing can take activation or focus. Leases are weak: a brush that
/// is collected, or retargeted, lets its source go; a layer goes with its last source.
/// </summary>
static partial class DetachedSourceHost
{
	static readonly List<HostLease> _leases = new();
	static Gtk.Fixed? _bare;
	static Gtk.Window? _bareWindow;
	static bool _sweeping;

	// GTK skips an opacity-0 subtree (its paintable renders nothing), so sources sit far outside the window instead.
	const double Offscreen = 100000;

	/// <summary>The never-mapped parent of sources hosted before (or without) any window.</summary>
	internal static Gtk.Fixed Bare
	{
		get
		{
			if (_bare != null) return _bare;
			// Realizing needs a toplevel above the widget; this one is never presented (no surface is shown).
			_bareWindow = new Gtk.Window { Child = _bare = new Gtk.Fixed() };
			return _bare;
		}
	}

	static void UnhostBare(HostLease lease)
	{
		if (!lease.SelfMapped) return;
		lease.SelfMapped = false;
		if (lease.Widget.Parent != _bare) return;
		lease.Widget.Unmap();
		_bare.Remove(lease.Widget);
	}

	internal static bool IsHosted(Gtk.Widget widget) => _leases.Any(l => l.Widget == widget);

	/// <summary>True while the lease still shows its source: its layer (or, with no window, itself) is mapped.</summary>
	internal static bool IsHealthy(HostLease lease)
	{
		if (!lease.Alive) return false;
		if (lease.Layer == null) return lease.MovePending || !AnyWindow();
		var layer = lease.Layer;
		if (layer.Parent == null || lease.Widget.Parent != layer) return false;
		if (layer.IsMapped) lease.SeenMapped = true;
		// A fresh layer maps with the next frame; one that was mapped and no longer is has lost its window.
		return layer.IsMapped || (!lease.SeenMapped && layer.Parent.IsMapped);
	}

	/// <summary>Hosts <paramref name="element"/> for <paramref name="owner"/>; null if it is parented elsewhere.</summary>
	internal static HostLease? Acquire(VisualBrush owner, UIElement element, SourceLink? link, Gtk.Widget? painter)
	{
		var widget = element.Widget;
		var shared = widget.Parent as Gtk.Fixed;
		if (widget.Parent != null && !IsLayer(shared)) return null;
		var lease = new HostLease(owner, widget, link) { Layer = shared };
		_leases.Add(lease);
		StartSweeping();
		if (shared != null) return lease;
		// Mapped by hand under a never-mapped parent (GTK draws nothing for a parentless or unmapped widget), which
		// is enough to render the first capture synchronously, with or without a window.
		Bare.Put(widget, 0, 0);
		widget.Realize();
		widget.Map();
		lease.SelfMapped = true;
		if (AnyWindow())
		{
			lease.MovePending = true;
			var root = painter?.Root as Gtk.Widget;
			GLib.Idle.Add(GLib.Priority.DefaultIdle, () => { Move(lease, root); return false; });
		}
		return lease;
	}

	static void Move(HostLease lease, Gtk.Widget? painterRoot)
	{
		lease.MovePending = false;
		if (!lease.Alive) return;
		if (lease.Widget.Parent is Gtk.Fixed other && IsLayer(other)) { lease.Layer = other; return; }
		if (lease.Widget.Parent != null && !lease.SelfMapped) return;
		var layer = EnsureLayer(painterRoot);
		if (layer == null) return;
		UnhostBare(lease);
		layer.Put(lease.Widget, -Offscreen, -Offscreen);
		lease.Layer = layer;
		if (lease.Owner.TryGetTarget(out var brush)) brush.OnSourceInvalidated();
	}

	internal static void Release(HostLease? lease)
	{
		if (lease == null || !lease.Alive) return;
		lease.Alive = false;
		_leases.Remove(lease);
		var layer = lease.Layer;
		bool shared = _leases.Any(l => l.Widget == lease.Widget);
		if (layer != null && lease.Widget.Parent == layer) { if (!shared) layer.Remove(lease.Widget); }
		else if (!shared) UnhostBare(lease);
		if (layer != null && _leases.All(l => l.Layer != layer)) DropLayer(layer);
	}

	/// <summary>Releases every lease whose brush has been collected.</summary>
	internal static void Sweep()
	{
		foreach (var lease in _leases.Where(l => !l.Owner.TryGetTarget(out _)).ToList())
		{
			lease.Link?.Dispose();
			Release(lease);
		}
	}

	static void StartSweeping()
	{
		if (_sweeping) return;
		_sweeping = true;
		GLib.Timeout.Add(2000, new GLib.TimeoutHandler(() =>
		{
			Sweep();
			_sweeping = _leases.Count > 0;
			return _sweeping;
		}));
	}
}
