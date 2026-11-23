namespace System.Windows.Controls;

public abstract partial class DataGridColumn
{
	/// <summary>Identifies <see cref="CellStyle"/>. Default null.</summary>
	public static readonly DependencyProperty CellStyleProperty = DependencyProperty.Register(
		nameof(CellStyle), typeof(Style), typeof(DataGridColumn), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="HeaderStyle"/>. Default null.</summary>
	public static readonly DependencyProperty HeaderStyleProperty = DependencyProperty.Register(
		nameof(HeaderStyle), typeof(Style), typeof(DataGridColumn), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="IsReadOnly"/>. Default false.</summary>
	public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
		nameof(IsReadOnly), typeof(bool), typeof(DataGridColumn), new PropertyMetadata(false));

	/// <summary>Gets or sets the style of this column's cells; it wins over <see cref="DataGrid.CellStyle"/>.</summary>
	public Style? CellStyle
	{
		get => (Style?)GetValue(CellStyleProperty);
		set => SetValue(CellStyleProperty, value);
	}

	/// <summary>Gets or sets the style of this column's header; it wins over <see cref="DataGrid.ColumnHeaderStyle"/>.</summary>
	public Style? HeaderStyle
	{
		get => (Style?)GetValue(HeaderStyleProperty);
		set => SetValue(HeaderStyleProperty, value);
	}

	/// <summary>Gets or sets whether the column's cells cannot be edited. Stored only.</summary>
	public bool IsReadOnly
	{
		get => (bool)GetValue(IsReadOnlyProperty)!;
		set => SetValue(IsReadOnlyProperty, value);
	}
}
