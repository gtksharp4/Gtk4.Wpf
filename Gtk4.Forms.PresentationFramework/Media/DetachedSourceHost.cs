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
}

/// <summary>
/// Parks unparented brush sources in an invisible layer inside a mapped application window (GTK draws nothing
/// for an unmapped widget). The layer is a non-targetable, non-focusable overlay child that holds its sources
/// far outside the window surface, so there is no extra toplevel and nothing can take activation or focus.
/// Leases are weak: a brush that is collected, or retargeted, lets its source go; the layer goes with the last.
/// </summary>
static class DetachedSourceHost
{
	static readonly List<HostLease> _leases = new();
	static Gtk.Overlay? _overlay;
	static Gtk.Fixed? _layer;
	static bool _sweeping;

	// GTK skips an opacity-0 subtree (its paintable renders nothing), so sources sit far outside the window instead.
	const double Offscreen = 100000;

	internal static bool IsHosted(Gtk.Widget widget) => _leases.Any(l => l.Widget == widget);

	/// <summary>Hosts <paramref name="element"/> for <paramref name="owner"/>; null when no window is mapped.</summary>
	internal static HostLease? Acquire(VisualBrush owner, UIElement element, SourceLink? link)
	{
		var layer = EnsureLayer();
		var widget = element.Widget;
		if (layer == null || (widget.Parent != null && widget.Parent != layer)) return null;
		if (widget.Parent == null) layer.Put(widget, -Offscreen, -Offscreen);
		var lease = new HostLease(owner, widget, link);
		_leases.Add(lease);
		StartSweeping();
		return lease;
	}

	internal static void Release(HostLease? lease)
	{
		if (lease == null || !lease.Alive) return;
		lease.Alive = false;
		_leases.Remove(lease);
		if (_layer != null && lease.Widget.Parent == _layer && _leases.All(l => l.Widget != lease.Widget))
			_layer.Remove(lease.Widget);
		if (_leases.Count == 0) DropLayer();
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

	static Gtk.Fixed? EnsureLayer()
	{
		if (_layer != null && _layer.Parent != null && _layer.IsMapped) return _layer;
		Discard();
		var overlay = Gtk.Window.ListToplevels()
			.Where(w => w.IsMapped).Select(w => w.Child as Gtk.Overlay).FirstOrDefault(o => o != null);
		if (overlay == null) return null;
		_layer = new Gtk.Fixed { CanTarget = false, CanFocus = false, Focusable = false };
		overlay.AddOverlay(_layer);
		overlay.SetMeasureOverlay(_layer, false);
		_overlay = overlay;
		return _layer;
	}

	// The hosting window went away: the sources are orphaned, so their brushes host them again on the next paint.
	static void Discard()
	{
		foreach (var lease in _leases) lease.Alive = false;
		_leases.Clear();
		if (_layer != null)
			foreach (var widget in _layer.Children.ToList()) _layer.Remove(widget);
		DropLayer();
	}

	static void DropLayer()
	{
		if (_layer != null && _layer.Parent == _overlay) _overlay?.RemoveOverlay(_layer);
		_layer = null;
		_overlay = null;
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
