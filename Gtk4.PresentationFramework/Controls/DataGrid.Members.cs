namespace System.Windows.Controls;

/// <summary>Which headers a <see cref="DataGrid"/> shows.</summary>
[Flags]
public enum DataGridHeadersVisibility
{
	/// <summary>No headers.</summary>
	None = 0,

	/// <summary>Column headers.</summary>
	Column = 1,

	/// <summary>Row headers.</summary>
	Row = 2,

	/// <summary>All headers, including the select-all corner.</summary>
	All = 3,
}

public partial class DataGrid
{
	/// <summary>Identifies <see cref="HeadersVisibility"/>. Default All.</summary>
	public static readonly DependencyProperty HeadersVisibilityProperty = DependencyProperty.Register(
		nameof(HeadersVisibility), typeof(DataGridHeadersVisibility), typeof(DataGrid),
		new FrameworkPropertyMetadata(DataGridHeadersVisibility.All),
		v => v is DataGridHeadersVisibility h && ((int)h & ~3) == 0);

	/// <summary>Identifies <see cref="CanUserResizeColumns"/>. Default true.</summary>
	public static readonly DependencyProperty CanUserResizeColumnsProperty = DependencyProperty.Register(
		nameof(CanUserResizeColumns), typeof(bool), typeof(DataGrid), new FrameworkPropertyMetadata(true));

	/// <summary>Identifies <see cref="CanUserResizeRows"/>. Default true.</summary>
	public static readonly DependencyProperty CanUserResizeRowsProperty = DependencyProperty.Register(
		nameof(CanUserResizeRows), typeof(bool), typeof(DataGrid), new FrameworkPropertyMetadata(true));

	/// <summary>Identifies <see cref="ColumnHeaderStyle"/>. Default null.</summary>
	public static readonly DependencyProperty ColumnHeaderStyleProperty = DependencyProperty.Register(
		nameof(ColumnHeaderStyle), typeof(Style), typeof(DataGrid),
		new FrameworkPropertyMetadata(null, (d, e) => ((DataGrid)d).RefreshCellsAndHeaders()));

	/// <summary>Gets or sets which headers are shown. Stored only: the GTK grid always shows its header row.</summary>
	public DataGridHeadersVisibility HeadersVisibility
	{
		get => (DataGridHeadersVisibility)GetValue(HeadersVisibilityProperty)!;
		set => SetValue(HeadersVisibilityProperty, value);
	}

	/// <summary>Gets or sets whether the user can resize columns. Stored only.</summary>
	public bool CanUserResizeColumns
	{
		get => (bool)GetValue(CanUserResizeColumnsProperty)!;
		set => SetValue(CanUserResizeColumnsProperty, value);
	}

	/// <summary>Gets or sets whether the user can resize rows. Stored only.</summary>
	public bool CanUserResizeRows
	{
		get => (bool)GetValue(CanUserResizeRowsProperty)!;
		set => SetValue(CanUserResizeRowsProperty, value);
	}

	/// <summary>Gets or sets the style for column headers. Stored only: header cells are plain GTK labels.</summary>
	public Style? ColumnHeaderStyle
	{
		get => (Style?)GetValue(ColumnHeaderStyleProperty);
		set => SetValue(ColumnHeaderStyleProperty, value);
	}
}
