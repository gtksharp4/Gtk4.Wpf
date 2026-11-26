namespace System.Windows.Controls;

public partial class DataGrid
{
	/// <summary>Identifies <see cref="CurrentItem"/>. Default null.</summary>
	public static readonly DependencyProperty CurrentItemProperty = DependencyProperty.Register(
		nameof(CurrentItem), typeof(object), typeof(DataGrid),
		new FrameworkPropertyMetadata(null, (d, e) => ((DataGrid)d).OnCurrentCellPartChanged()));

	/// <summary>Identifies <see cref="CurrentColumn"/>. Default null.</summary>
	public static readonly DependencyProperty CurrentColumnProperty = DependencyProperty.Register(
		nameof(CurrentColumn), typeof(DataGridColumn), typeof(DataGrid),
		new FrameworkPropertyMetadata(null, (d, e) => ((DataGrid)d).OnCurrentCellPartChanged()));

	bool _settingCurrentCell;

	/// <summary>Occurs when <see cref="CurrentCell"/> changes.</summary>
	public event EventHandler<EventArgs>? CurrentCellChanged;

	/// <summary>Gets or sets the row item of the current cell.</summary>
	public object? CurrentItem
	{
		get => GetValue(CurrentItemProperty);
		set => SetValue(CurrentItemProperty, value);
	}

	/// <summary>Gets or sets the column of the current cell.</summary>
	public DataGridColumn? CurrentColumn
	{
		get => (DataGridColumn?)GetValue(CurrentColumnProperty);
		set => SetValue(CurrentColumnProperty, value);
	}

	/// <summary>Gets or sets the current cell; an invalid info clears it.</summary>
	public DataGridCellInfo CurrentCell
	{
		get => CurrentColumn != null
			? new DataGridCellInfo(CurrentItem!, CurrentColumn)
			: default(DataGridCellInfo);
		set
		{
			if (value == CurrentCell) return;
			_settingCurrentCell = true;
			try
			{
				CurrentItem = value.IsValid ? value.Item : null;
				CurrentColumn = value.Column;
			}
			finally { _settingCurrentCell = false; }
			CurrentCellChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	void OnCurrentCellPartChanged()
	{
		if (!_settingCurrentCell) CurrentCellChanged?.Invoke(this, EventArgs.Empty);
	}
}
