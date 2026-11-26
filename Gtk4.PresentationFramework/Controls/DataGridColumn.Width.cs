namespace System.Windows.Controls;

public abstract partial class DataGridColumn
{
	/// <summary>Identifies <see cref="Width"/>. Default <see cref="DataGridLength.Auto"/>.</summary>
	public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
		nameof(Width), typeof(DataGridLength), typeof(DataGridColumn),
		new PropertyMetadata(DataGridLength.Auto, OnWidthChanged, OnCoerceWidth));

	/// <summary>
	/// Gets or sets the column width. Until set locally (or by a style) it follows
	/// <see cref="DataGrid.ColumnWidth"/> of the owning grid.
	/// </summary>
	public DataGridLength Width
	{
		get => (DataGridLength)GetValue(WidthProperty)!;
		set => SetValue(WidthProperty, value);
	}

	static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((DataGridColumn)d).DataGridOwner?.ApplyColumnWidths();

	// Transfer: a column with no value of its own takes the grid's ColumnWidth.
	static object? OnCoerceWidth(DependencyObject d, object? baseValue)
	{
		var column = (DataGridColumn)d;
		var grid = column.DataGridOwner;
		return grid != null && !column.HasNonInheritedValue(WidthProperty) ? grid.ColumnWidth : baseValue;
	}
}
