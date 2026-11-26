using System.Windows.Input;

namespace System.Windows;

/// <summary>
/// <c>PreviewMouseWheel</c>/<c>MouseWheel</c> events, fed by a GTK 4
/// <see cref="Gtk.EventControllerScroll"/>.
/// </summary>
/// <remarks>
/// GTK delivers vertical scroll as <c>dy</c> (positive = down): in wheel units (1.0 per notch,
/// fractional for smooth scrolling) or, for touchpads/precise scrolling, in surface pixels
/// (<c>Gtk.EventControllerScroll.Unit</c> = <c>Surface</c>, converted at
/// <see cref="MouseWheelMap.SurfacePixelsPerNotch"/> px per notch);
/// <see cref="MouseWheelMap.ToDelta(double, bool)"/> converts it to WPF's 120-per-notch delta.
/// A delta that rounds to 0 raises nothing. Horizontal-only scrolling produces no WPF event (WPF's <c>MouseWheel</c>
/// is vertical). The controller's return value is the WPF <c>Handled</c> flag, so an unhandled
/// wheel still reaches <c>Gtk.ScrolledWindow</c> and a <c>ScrollViewer</c> keeps scrolling.
/// </remarks>
public abstract partial class UIElement
{
	/// <summary>Tunnel PreviewMouseWheel.</summary>
	public static readonly RoutedEvent PreviewMouseWheelEvent =
		EventManager.RegisterRoutedEvent("PreviewMouseWheel", RoutingStrategy.Tunnel,
			typeof(MouseWheelEventHandler), typeof(UIElement));

	/// <summary>Bubble MouseWheel.</summary>
	public static readonly RoutedEvent MouseWheelEvent =
		EventManager.RegisterRoutedEvent("MouseWheel", RoutingStrategy.Bubble,
			typeof(MouseWheelEventHandler), typeof(UIElement));

	/// <summary>Occurs when the wheel rotates (tunnel phase).</summary>
	public event MouseWheelEventHandler? PreviewMouseWheel
	{
		add    => AddHandler(PreviewMouseWheelEvent, value!);
		remove => RemoveHandler(PreviewMouseWheelEvent, value!);
	}

	/// <summary>Occurs when the wheel rotates while the pointer is over this element.</summary>
	public event MouseWheelEventHandler? MouseWheel
	{
		add    => AddHandler(MouseWheelEvent, value!);
		remove => RemoveHandler(MouseWheelEvent, value!);
	}

	void AttachScrollController(Gtk.Widget widget)
	{
		var scroll = new Gtk.EventControllerScroll(
			Gtk.EventControllerScrollFlags.Vertical | Gtk.EventControllerScrollFlags.Horizontal);
		scroll.Scroll += OnGtkScroll;
		widget.AddController(scroll);
	}

	void OnGtkScroll(object o, Gtk.ScrollArgs args)
	{
		// Touchpads/precise scrolling report pixels (surface unit); WPF's delta is in wheel units.
		bool surface = o is Gtk.EventControllerScroll sc && sc.Unit == Gdk.ScrollUnit.Surface;
		int delta = MouseWheelMap.ToDelta(args.Dy, surface);
		// Each nested element's controller sees the same GDK event while it propagates; the
		// WPF route already bubbled it, so later controllers only pass the handled state on.
		if (delta == 0 || IsDuplicateDispatch(o))
		{
			args.RetVal = false;
			return;
		}
		args.RetVal = RaiseMouseWheel(delta, GdkEventTime(o));
	}

	/// <summary>
	/// Raises PreviewMouseWheel (tunnel) then, unless handled, MouseWheel (bubble) on the input
	/// target (this element, or the captured element). Returns whether either was handled — the
	/// value the GTK scroll controller returns.
	/// </summary>
	internal bool RaiseMouseWheel(int delta, int? timestamp = null)
	{
		int ts = timestamp ?? Environment.TickCount;
		var target = Mouse.GetInputTarget(this);
		var pos = Mouse._lastPosition;
		var preview = new MouseWheelEventArgs(PreviewMouseWheelEvent, pos, delta,
			Mouse.LeftButton, Mouse.RightButton, Keyboard.Modifiers, ts) { PositionSource = Mouse._lastPositionSource };
		target.RaiseEvent(preview);
		if (preview.Handled) return true;

		var e = new MouseWheelEventArgs(MouseWheelEvent, pos, delta,
			Mouse.LeftButton, Mouse.RightButton, Keyboard.Modifiers, ts) { PositionSource = Mouse._lastPositionSource };
		target.RaiseEvent(e);
		return e.Handled;
	}
}
