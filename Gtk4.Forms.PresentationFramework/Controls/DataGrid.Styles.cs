namespace System.Windows.Controls;

public partial class DataGrid
{
	/// <summary>Identifies <see cref="CanUserAddRows"/>. Default true.</summary>
	public static readonly DependencyProperty CanUserAddRowsProperty = DependencyProperty.Register(
		nameof(CanUserAddRows), typeof(bool), typeof(DataGrid),
		new FrameworkPropertyMetadata(true, null, CoerceCanUserAddRows));

	/// <summary>Identifies <see cref="CellStyle"/>. Default null.</summary>
	public static readonly DependencyProperty CellStyleProperty = DependencyProperty.Register(
		nameof(CellStyle), typeof(Style), typeof(DataGrid), new FrameworkPropertyMetadata(null));

	/// <summary>Identifies <see cref="IsReadOnly"/>. Default false.</summary>
	public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
		nameof(IsReadOnly), typeof(bool), typeof(DataGrid),
		new FrameworkPropertyMetadata(false, (d, e) => d.CoerceValue(CanUserAddRowsProperty)));

	// WPF: a read-only grid cannot add rows. The source-side checks (IEditableCollectionView.CanAddNew) are not
	// modelled because the GTK grid keeps a plain snapshot of the items.
	static object? CoerceCanUserAddRows(DependencyObject d, object? value) =>
		(bool)value! && !((DataGrid)d).IsReadOnly;

	/// <summary>Gets or sets whether the user can add rows. A read-only grid reports false. Stored only.</summary>
	public bool CanUserAddRows
	{
		get => (bool)GetValue(CanUserAddRowsProperty)!;
		set => SetValue(CanUserAddRowsProperty, value);
	}

	/// <summary>Gets or sets the style of the cells; a column's own <see cref="DataGridColumn.CellStyle"/> wins.</summary>
	public Style? CellStyle
	{
		get => (Style?)GetValue(CellStyleProperty);
		set => SetValue(CellStyleProperty, value);
	}

	/// <summary>Gets or sets whether the cells cannot be edited. Stored only.</summary>
	public bool IsReadOnly
	{
		get => (bool)GetValue(IsReadOnlyProperty)!;
		set => SetValue(IsReadOnlyProperty, value);
	}
}
