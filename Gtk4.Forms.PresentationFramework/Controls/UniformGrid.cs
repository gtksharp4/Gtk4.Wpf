using System;

namespace System.Windows.Controls.Primitives;

/// <summary>
/// Arranges children in a uniform grid of equal-sized cells.
/// <para>
/// GTK4 backing: <see cref="Gtk.Grid"/> (homogeneous row and column sizing).
/// </para>
/// <para>
/// Row/column count auto-computation: when both are 0, columns = ceil(sqrt(n)),
/// rows = ceil(n / columns). When only one dimension is set the other is derived.
/// </para>
/// </summary>
public partial class UniformGrid : Panel
{
	// ── Rows DP ───────────────────────────────────────────────────────────────

	/// <summary>Number of rows. 0 = auto-compute.</summary>
	public static readonly DependencyProperty RowsProperty =
		DependencyProperty.Register(
			nameof(Rows),
			typeof(int),
			typeof(UniformGrid),
			new PropertyMetadata(0, OnLayoutChanged));

	/// <summary>Gets or sets the number of rows (0 = auto).</summary>
	public int Rows
	{
		get => (int?)GetValue(RowsProperty) ?? 0;
		set => SetValue(RowsProperty, value);
	}

	// ── Columns DP ────────────────────────────────────────────────────────────

	/// <summary>Number of columns. 0 = auto-compute.</summary>
	public static readonly DependencyProperty ColumnsProperty =
		DependencyProperty.Register(
			nameof(Columns),
			typeof(int),
			typeof(UniformGrid),
			new PropertyMetadata(0, OnLayoutChanged));

	/// <summary>Gets or sets the number of columns (0 = auto).</summary>
	public int Columns
	{
		get => (int?)GetValue(ColumnsProperty) ?? 0;
		set => SetValue(ColumnsProperty, value);
	}

	static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((UniformGrid)d).RebuildLayout();
	}

	// ── Layout computation ────────────────────────────────────────────────────

	/// <summary>
	/// Computes (rows, cols) from explicit settings and child count.
	/// </summary>
	internal static (int rows, int cols) ComputeGrid(int explicitRows, int explicitCols, int childCount)
	{
		if (childCount == 0) return (1, 1);

		if (explicitRows > 0 && explicitCols > 0)
			return (explicitRows, explicitCols);

		if (explicitRows > 0)
		{
			int cols = (childCount + explicitRows - 1) / explicitRows;
			return (explicitRows, System.Math.Max(1, cols));
		}

		if (explicitCols > 0)
		{
			int rows = (childCount + explicitCols - 1) / explicitCols;
			return (System.Math.Max(1, rows), explicitCols);
		}

		// Both 0: auto
		int autoCols = (int)System.Math.Ceiling(System.Math.Sqrt(childCount));
		autoCols = System.Math.Max(1, autoCols);
		int autoRows = (childCount + autoCols - 1) / autoCols;
		return (autoRows, autoCols);
	}

	// ── GTK backing ──────────────────────────────────────────────────────────

	Gtk.Grid? _grid;

	/// <inheritdoc/>
	internal override bool IsWidgetCreated => _grid != null;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_grid = new Gtk.Grid();
		_grid.RowHomogeneous = true;
		_grid.ColumnHomogeneous = true;
		RebuildLayout();
		return _grid;
	}

	/// <inheritdoc/>
	protected override void AddChildWidget(UIElement child)
	{
		// A new child invalidates the layout; rebuild positions.
		RebuildLayout();
	}

	/// <inheritdoc/>
	protected override void RemoveChildWidget(UIElement child)
	{
		if (_grid != null && _attached.Remove(child))
			_grid.Remove(child.Widget);
		RebuildLayout();
	}

	// Tracks which children are currently attached to the Gtk.Grid.
	readonly System.Collections.Generic.HashSet<UIElement> _attached =
		new System.Collections.Generic.HashSet<UIElement>();

	void RebuildLayout()
	{
		if (_grid == null) return;

		// Detach only previously-attached children to avoid GTK warnings on unattached widgets.
		foreach (var child in _attached)
			_grid.Remove(child.Widget);
		_attached.Clear();

		var (rows, cols) = ComputeGrid(Rows, Columns, Children.Count);
		int capacity = rows * cols;

		for (int i = 0; i < Children.Count; i++)
		{
			// R7: clamp to available grid cells — overflow children are skipped.
			if (i >= capacity) break;
			int col = i % cols;
			int row = i / cols;
			_grid.Attach(Children[i].Widget, col, row, 1, 1);
			_attached.Add(Children[i]);
		}
	}
}