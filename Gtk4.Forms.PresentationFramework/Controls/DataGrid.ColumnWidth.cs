using System.Collections.Generic;

namespace System.Windows.Controls;

public partial class DataGrid
{
	/// <summary>Identifies <see cref="ColumnWidth"/>. Default <see cref="DataGridLength.SizeToHeader"/>.</summary>
	public static readonly DependencyProperty ColumnWidthProperty = DependencyProperty.Register(
		nameof(ColumnWidth), typeof(DataGridLength), typeof(DataGrid),
		new FrameworkPropertyMetadata(DataGridLength.SizeToHeader, OnColumnWidthChanged));

	/// <summary>Identifies <see cref="MinColumnWidth"/>. Default 20.</summary>
	public static readonly DependencyProperty MinColumnWidthProperty = DependencyProperty.Register(
		nameof(MinColumnWidth), typeof(double), typeof(DataGrid),
		new FrameworkPropertyMetadata(20.0, OnColumnWidthChanged),
		v => { var d = (double)v!; return !(double.IsNaN(d) || d < 0 || double.IsPositiveInfinity(d)); });

	/// <summary>Identifies <see cref="MaxColumnWidth"/>. Default +infinity.</summary>
	public static readonly DependencyProperty MaxColumnWidthProperty = DependencyProperty.Register(
		nameof(MaxColumnWidth), typeof(double), typeof(DataGrid),
		new FrameworkPropertyMetadata(double.PositiveInfinity, OnColumnWidthChanged),
		v => { var d = (double)v!; return !(double.IsNaN(d) || d < 0); });

	/// <summary>Gets or sets the width columns take when they have none of their own.</summary>
	public DataGridLength ColumnWidth
	{
		get => (DataGridLength)GetValue(ColumnWidthProperty)!;
		set => SetValue(ColumnWidthProperty, value);
	}

	/// <summary>Gets or sets the minimum column width. Stored only.</summary>
	public double MinColumnWidth
	{
		get => (double)GetValue(MinColumnWidthProperty)!;
		set => SetValue(MinColumnWidthProperty, value);
	}

	/// <summary>Gets or sets the maximum column width. Stored only.</summary>
	public double MaxColumnWidth
	{
		get => (double)GetValue(MaxColumnWidthProperty)!;
		set => SetValue(MaxColumnWidthProperty, value);
	}

	static void OnColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var grid = (DataGrid)d;
		foreach (var column in grid._ownedColumns) column.CoerceValue(DataGridColumn.WidthProperty);
		grid.ApplyColumnWidths();
	}

	// GTK Grid shares spare width equally, so star weights other than equal are store-only, and
	// SizeToCells and SizeToHeader both map to natural size. A Pixel width is a minimum size request on the
	// header widget only (cells just get Hexpand off); a GTK request is a minimum, so wider header or cell
	// content makes the column wider, where WPF clips to the exact width.
	internal void ApplyColumnWidths()
	{
		if (_grid == null) return;
		var cols = GetEffectiveColumns();
		for (int c = 0; c < cols.Count; c++)
		{
			var width = cols[c].Width;
			var request = width.IsAbsolute ? (int)Math.Round(width.Value) : -1;
			var header = _grid.GetChildAt(c, 0);
			header?.SetSizeRequest(request, -1);
			for (int r = 0; _grid.GetChildAt(c, r) is Gtk.Widget cell; r++)
				cell.Hexpand = width.IsStar;
		}
	}
}
