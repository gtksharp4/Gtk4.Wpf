using System;

namespace System.Windows.Controls.Primitives;

/// <summary>Event data for <see cref="Thumb.DragStarted"/>.</summary>
public sealed class DragStartedEventArgs : EventArgs
{
	/// <summary>Horizontal offset at drag start.</summary>
	public double HorizontalOffset { get; }
	/// <summary>Vertical offset at drag start.</summary>
	public double VerticalOffset { get; }

	internal DragStartedEventArgs(double h, double v) { HorizontalOffset = h; VerticalOffset = v; }
}

/// <summary>Event data for <see cref="Thumb.DragDelta"/>.</summary>
public sealed class DragDeltaEventArgs : EventArgs
{
	/// <summary>Horizontal change since last event.</summary>
	public double HorizontalChange { get; }
	/// <summary>Vertical change since last event.</summary>
	public double VerticalChange { get; }

	internal DragDeltaEventArgs(double h, double v) { HorizontalChange = h; VerticalChange = v; }
}

/// <summary>Event data for <see cref="Thumb.DragCompleted"/>.</summary>
public sealed class DragCompletedEventArgs : EventArgs
{
	/// <summary>Total horizontal distance dragged.</summary>
	public double HorizontalChange { get; }
	/// <summary>Total vertical distance dragged.</summary>
	public double VerticalChange { get; }
	/// <summary>True if the drag was cancelled.</summary>
	public bool Canceled { get; }

	internal DragCompletedEventArgs(double h, double v, bool canceled)
	{
		HorizontalChange = h; VerticalChange = v; Canceled = canceled;
	}
}

/// <summary>
/// A draggable handle control that reports <see cref="DragStarted"/>,
/// <see cref="DragDelta"/> and <see cref="DragCompleted"/> events.
/// Backed by a <see cref="Gtk.Box"/> with a <see cref="Gtk.GestureDrag"/> controller.
/// </summary>
public class Thumb : Control
{
	// ── Drag events ──────────────────────────────────────────────────────────

	/// <summary>Raised when a drag gesture begins.</summary>
	public event EventHandler<DragStartedEventArgs>? DragStarted;

	/// <summary>Raised on each drag movement.</summary>
	public event EventHandler<DragDeltaEventArgs>? DragDelta;

	/// <summary>Raised when the drag gesture ends.</summary>
	public event EventHandler<DragCompletedEventArgs>? DragCompleted;

	// ── GTK backing ──────────────────────────────────────────────────────────

	Gtk.Box? _box;
	Gtk.GestureDrag? _drag;
	double _startX, _startY;
	double _prevX, _prevY;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_box = new Gtk.Box(Gtk.Orientation.Horizontal, 0);
		_box.AddCssClass("thumb");

		_drag = new Gtk.GestureDrag();
		_drag.DragStarted += OnDragBegin;
		_drag.DragUpdate += OnDragUpdate;
		_drag.DragEnded += OnDragEnd;
		_box.AddController(_drag);

		return _box;
	}

	void OnDragBegin(object o, Gtk.DragStartedArgs e)
	{
		_startX = e.StartX;
		_startY = e.StartY;
		_prevX = 0;
		_prevY = 0;
		DragStarted?.Invoke(this, new DragStartedEventArgs(e.StartX, e.StartY));
	}

	void OnDragUpdate(object o, Gtk.DragUpdateArgs e)
	{
		double dx = e.OffsetX - _prevX;
		double dy = e.OffsetY - _prevY;
		_prevX = e.OffsetX;
		_prevY = e.OffsetY;
		DragDelta?.Invoke(this, new DragDeltaEventArgs(dx, dy));
	}

	void OnDragEnd(object o, Gtk.DragEndedArgs e)
	{
		_drag!.GetOffset(out var ox, out var oy);
		DragCompleted?.Invoke(this, new DragCompletedEventArgs(ox, oy, false));
	}
}