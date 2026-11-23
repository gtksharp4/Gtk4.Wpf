namespace System.Windows.Controls;

/// <summary>
/// A cell of a <see cref="DataGrid"/>. The grid builds one per row and column and hosts a
/// <see cref="TextBlock"/> with the column's text as its <see cref="ContentControl.Content"/>.
/// </summary>
public class DataGridCell : ContentControl
{
	static readonly DependencyPropertyKey _columnKey = DependencyProperty.RegisterReadOnly(
		nameof(Column), typeof(DataGridColumn), typeof(DataGridCell), new FrameworkPropertyMetadata(null));

	static readonly DependencyPropertyKey _isReadOnlyKey = DependencyProperty.RegisterReadOnly(
		nameof(IsReadOnly), typeof(bool), typeof(DataGridCell), new FrameworkPropertyMetadata(false));

	/// <summary>Identifies the read-only <see cref="Column"/> property.</summary>
	public static readonly DependencyProperty ColumnProperty = _columnKey.DependencyProperty;

	/// <summary>Identifies <see cref="IsSelected"/>. Default false.</summary>
	public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
		nameof(IsSelected), typeof(bool), typeof(DataGridCell), new FrameworkPropertyMetadata(false));

	/// <summary>Identifies <see cref="IsEditing"/>. Default false.</summary>
	public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register(
		nameof(IsEditing), typeof(bool), typeof(DataGridCell), new FrameworkPropertyMetadata(false));

	/// <summary>Identifies the read-only <see cref="IsReadOnly"/> property.</summary>
	public static readonly DependencyProperty IsReadOnlyProperty = _isReadOnlyKey.DependencyProperty;

	static DataGridCell()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridCell), new PropertyMetadata(typeof(DataGridCell)));
		FocusableProperty.OverrideMetadata(typeof(DataGridCell), new FrameworkPropertyMetadata(true));
	}

	/// <summary>Gets the column the cell belongs to.</summary>
	public DataGridColumn? Column
	{
		get => (DataGridColumn?)GetValue(ColumnProperty);
		internal set => SetValue(_columnKey, value);
	}

	/// <summary>Gets or sets whether the cell is selected.</summary>
	public bool IsSelected
	{
		get => (bool)GetValue(IsSelectedProperty)!;
		set => SetValue(IsSelectedProperty, value);
	}

	/// <summary>Gets or sets whether the cell is being edited. Stored only: the GTK grid has no cell editing.</summary>
	public bool IsEditing
	{
		get => (bool)GetValue(IsEditingProperty)!;
		set => SetValue(IsEditingProperty, value);
	}

	/// <summary>Gets whether the cell cannot be edited (its column is read-only).</summary>
	public bool IsReadOnly
	{
		get => (bool)GetValue(IsReadOnlyProperty)!;
		internal set => SetValue(_isReadOnlyKey, value);
	}

	/// <summary>The item of the row the cell is in.</summary>
	internal object? RowItem { get; set; }

	internal bool RowClickWired { get; set; }
}
