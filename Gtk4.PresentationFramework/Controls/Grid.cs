namespace System.Windows.Controls;

/// <summary>
/// Phase-1 Grid: backs with a <see cref="Gtk.Grid"/> (GTK 4), honouring
/// <see cref="ColumnDefinitions"/>/<see cref="RowDefinitions"/> and the attached
/// Row/Column/RowSpan/ColumnSpan properties.
///
/// GTK backing approximations:
/// - Star and Auto rows/columns have no native GTK equivalent; all are mapped to GTK's
///   default expand/fill behaviour. Star columns/rows get Hexpand=true; Auto uses natural
///   size. Pixel rows/columns request a fixed size via SizeRequest on a hidden size-label
///   (not yet wired — noted as Phase 2).  For now, pixel defs are treated as Auto.
/// - Span is passed directly to Gtk.Grid.Attach(col, row, colSpan, rowSpan).
/// </summary>
public partial class Grid : Panel
{
	// ── Attached properties ──────────────────────────────────────────────────────

	/// <summary>Attached: zero-based row index.</summary>
	public static readonly DependencyProperty RowProperty =
		DependencyProperty.RegisterAttached("Row", typeof(int), typeof(Grid),
			new PropertyMetadata(0, OnCellPropertyChanged));

	/// <summary>Attached: zero-based column index.</summary>
	public static readonly DependencyProperty ColumnProperty =
		DependencyProperty.RegisterAttached("Column", typeof(int), typeof(Grid),
			new PropertyMetadata(0, OnCellPropertyChanged));

	/// <summary>Attached: number of rows the child spans (≥1).</summary>
	public static readonly DependencyProperty RowSpanProperty =
		DependencyProperty.RegisterAttached("RowSpan", typeof(int), typeof(Grid),
			new PropertyMetadata(1, OnCellPropertyChanged));

	/// <summary>Attached: number of columns the child spans (≥1).</summary>
	public static readonly DependencyProperty ColumnSpanProperty =
		DependencyProperty.RegisterAttached("ColumnSpan", typeof(int), typeof(Grid),
			new PropertyMetadata(1, OnCellPropertyChanged));

	/// <summary>Gets the row index of <paramref name="element"/>.</summary>
	public static int GetRow(UIElement element) => (int)element.GetValue(RowProperty)!;
	/// <summary>Sets the row index of <paramref name="element"/>.</summary>
	public static void SetRow(UIElement element, int value) => element.SetValue(RowProperty, value);

	/// <summary>Gets the column index of <paramref name="element"/>.</summary>
	public static int GetColumn(UIElement element) => (int)element.GetValue(ColumnProperty)!;
	/// <summary>Sets the column index of <paramref name="element"/>.</summary>
	public static void SetColumn(UIElement element, int value) => element.SetValue(ColumnProperty, value);

	/// <summary>Gets the row span of <paramref name="element"/>.</summary>
	public static int GetRowSpan(UIElement element) => (int)element.GetValue(RowSpanProperty)!;
	/// <summary>Sets the row span of <paramref name="element"/>.</summary>
	public static void SetRowSpan(UIElement element, int value) => element.SetValue(RowSpanProperty, value);

	/// <summary>Gets the column span of <paramref name="element"/>.</summary>
	public static int GetColumnSpan(UIElement element) => (int)element.GetValue(ColumnSpanProperty)!;
	/// <summary>Sets the column span of <paramref name="element"/>.</summary>
	public static void SetColumnSpan(UIElement element, int value) => element.SetValue(ColumnSpanProperty, value);

	// ── Row / Column definitions ─────────────────────────────────────────────────

	/// <summary>Column width definitions. An empty collection means one auto column.</summary>
	public ColumnDefinitionCollection ColumnDefinitions { get; } = new ColumnDefinitionCollection();

	/// <summary>Row height definitions. An empty collection means one auto row.</summary>
	public RowDefinitionCollection RowDefinitions { get; } = new RowDefinitionCollection();

	// ── GTK backing ──────────────────────────────────────────────────────────────

	Gtk.Grid? _grid;

	/// <inheritdoc/>
	internal override bool IsWidgetCreated => _grid != null;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_grid = new BackdropGrid();
		_grid.RowHomogeneous = false;
		_grid.ColumnHomogeneous = false;
		foreach (var child in Children)
			AttachChild(child);
		return _grid;
	}

	/// <inheritdoc/>
	protected override void AddChildWidget(UIElement child)
	{
		AttachChild(child);
	}

	/// <inheritdoc/>
	protected override void RemoveChildWidget(UIElement child)
	{
		_grid!.Remove(child.Widget);
	}

	void AttachChild(UIElement child)
	{
		int col = GetColumn(child);
		int row = GetRow(child);
		int colSpan = System.Math.Max(1, GetColumnSpan(child));
		int rowSpan = System.Math.Max(1, GetRowSpan(child));
		_grid!.Attach(child.Widget, col, row, colSpan, rowSpan);

		// Apply expand hints based on definitions (best effort).
		ApplyExpandHints(child);
	}

	void ApplyExpandHints(UIElement child)
	{
		int col = GetColumn(child);
		int row = GetRow(child);
		int colSpan = System.Math.Max(1, GetColumnSpan(child));
		int rowSpan = System.Math.Max(1, GetRowSpan(child));

		bool hExpand = ShouldExpandH(col, colSpan);
		bool vExpand = ShouldExpandV(row, rowSpan);

		child.Widget.Hexpand = hExpand;
		child.Widget.Vexpand = vExpand;
	}

	bool ShouldExpandH(int col, int span)
	{
		if (ColumnDefinitions.Count == 0) return true; // default: one star col
		for (int i = col; i < col + span && i < ColumnDefinitions.Count; i++)
		{
			if (ColumnDefinitions[i].Width.IsStar) return true;
		}
		return false;
	}

	bool ShouldExpandV(int row, int span)
	{
		if (RowDefinitions.Count == 0) return true; // default: one star row
		for (int i = row; i < row + span && i < RowDefinitions.Count; i++)
		{
			if (RowDefinitions[i].Height.IsStar) return true;
		}
		return false;
	}

	// A track length changed after its children were measured: their slot caps read it, so re-measure them.
	internal void OnTrackLengthChanged() => RefreshSlotCaps(this);

	// When an attached cell property changes, re-attach the child.
	static void OnCellPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is UIElement child && child.LogicalParent is Grid grid && grid._grid != null)
		{
			grid._grid.Remove(child.Widget);
			grid.AttachChild(child);
		}
	}
}