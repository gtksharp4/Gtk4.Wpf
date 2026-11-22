using System;

namespace System.Windows.Controls;

/// <summary>Specifies which columns or rows a <see cref="GridSplitter"/> resizes.</summary>
public enum GridResizeBehavior
{
	/// <summary>Chosen automatically based on alignment.</summary>
	BasedOnAlignment,
	/// <summary>Both adjacent columns/rows share the resize.</summary>
	CurrentAndNext,
	/// <summary>Only the previous column/row is resized.</summary>
	PreviousAndCurrent,
	/// <summary>Both adjacent columns/rows are resized.</summary>
	PreviousAndNext,
}

/// <summary>Specifies which direction a <see cref="GridSplitter"/> resizes.</summary>
public enum GridResizeDirection
{
	/// <summary>Chosen automatically based on alignment.</summary>
	Auto,
	/// <summary>Resizes columns.</summary>
	Columns,
	/// <summary>Resizes rows.</summary>
	Rows,
}

/// <summary>
/// A resizer bar that lets the user resize rows or columns in a <see cref="Grid"/>.
/// <para>
/// GTK4 backing: a <see cref="Gtk.Box"/> (thin bar) with a <see cref="Gtk.GestureDrag"/>
/// controller. Because GTK4 has no <c>Gtk.Paned</c> <em>column</em>-splitter integrated
/// into a <c>Gtk.Grid</c>, this control emits <see cref="DragDelta"/> so host code can
/// resize column/row definitions programmatically. Visual grab handle only; no automatic
/// Grid column/row resize (Phase-2 work).
/// </para>
/// </summary>
public partial class GridSplitter : Control
{
	// ── ResizeDirection DP ───────────────────────────────────────────────────

	/// <summary>Direction of resize.</summary>
	public static readonly DependencyProperty ResizeDirectionProperty =
		DependencyProperty.Register(
			nameof(ResizeDirection),
			typeof(GridResizeDirection),
			typeof(GridSplitter),
			new PropertyMetadata(GridResizeDirection.Auto));

	/// <summary>Gets or sets the resize direction.</summary>
	public GridResizeDirection ResizeDirection
	{
		get => (GridResizeDirection?)GetValue(ResizeDirectionProperty) ?? GridResizeDirection.Auto;
		set => SetValue(ResizeDirectionProperty, value);
	}

	// ── ResizeBehavior DP ────────────────────────────────────────────────────

	/// <summary>Which adjacent cells are resized.</summary>
	public static readonly DependencyProperty ResizeBehaviorProperty =
		DependencyProperty.Register(
			nameof(ResizeBehavior),
			typeof(GridResizeBehavior),
			typeof(GridSplitter),
			new PropertyMetadata(GridResizeBehavior.BasedOnAlignment));

	/// <summary>Gets or sets the resize behavior.</summary>
	public GridResizeBehavior ResizeBehavior
	{
		get => (GridResizeBehavior?)GetValue(ResizeBehaviorProperty) ?? GridResizeBehavior.BasedOnAlignment;
		set => SetValue(ResizeBehaviorProperty, value);
	}

	// ── DragDelta event ──────────────────────────────────────────────────────

	/// <summary>Raised on each drag movement; consumers may resize Grid definitions.</summary>
	public event EventHandler<Primitives.DragDeltaEventArgs>? DragDelta;

	// ── GTK backing ──────────────────────────────────────────────────────────

	Gtk.Box? _bar;
	Gtk.GestureDrag? _drag;
	double _prevX, _prevY;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_bar = new Gtk.Box(Gtk.Orientation.Horizontal, 0);
		_bar.AddCssClass("gridsplitter");

		_drag = new Gtk.GestureDrag();
		_drag.DragStarted += (o, e) => { _prevX = 0; _prevY = 0; };
		_drag.DragUpdate += OnDragUpdate;
		_bar.AddController(_drag);

		return _bar;
	}

	void OnDragUpdate(object o, Gtk.DragUpdateArgs e)
	{
		double dx = e.OffsetX - _prevX;
		double dy = e.OffsetY - _prevY;
		_prevX = e.OffsetX;
		_prevY = e.OffsetY;
		DragDelta?.Invoke(this, new Primitives.DragDeltaEventArgs(dx, dy));
	}
}