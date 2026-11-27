using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>
/// A <see cref="ViewBase"/> that shows a <see cref="ListView"/> as a table: a header row of
/// <see cref="GridViewColumnHeader"/>s and one <see cref="GridViewRowPresenter"/> per item. A column with an
/// explicit <see cref="GridViewColumn.Width"/> is that wide; the others share one width per column between the
/// header and every row (a GTK size group).
/// </summary>
[ContentProperty("Columns")]
public class GridView : ViewBase
{
	/// <summary>Identifies the <see cref="ColumnHeaderContainerStyle"/> property.</summary>
	public static readonly DependencyProperty ColumnHeaderContainerStyleProperty =
		Reg(nameof(ColumnHeaderContainerStyle), typeof(Style), null);

	/// <summary>Identifies the <see cref="ColumnHeaderTemplate"/> property.</summary>
	public static readonly DependencyProperty ColumnHeaderTemplateProperty =
		Reg(nameof(ColumnHeaderTemplate), typeof(DataTemplate), null);

	/// <summary>Identifies the <see cref="ColumnHeaderTemplateSelector"/> property.</summary>
	public static readonly DependencyProperty ColumnHeaderTemplateSelectorProperty =
		Reg(nameof(ColumnHeaderTemplateSelector), typeof(DataTemplateSelector), null);

	/// <summary>Identifies the <see cref="ColumnHeaderStringFormat"/> property.</summary>
	public static readonly DependencyProperty ColumnHeaderStringFormatProperty =
		Reg(nameof(ColumnHeaderStringFormat), typeof(string), null);

	/// <summary>Identifies the <see cref="AllowsColumnReorder"/> property (stored only; there is no drag).</summary>
	public static readonly DependencyProperty AllowsColumnReorderProperty =
		Reg(nameof(AllowsColumnReorder), typeof(bool), true);

	readonly GridViewColumnCollection _columns = new GridViewColumnCollection();
	readonly Dictionary<GridViewColumn, Gtk.SizeGroup> _sizeGroups = new Dictionary<GridViewColumn, Gtk.SizeGroup>();
	readonly List<WeakReference<GridViewRowPresenter>> _presenters = new List<WeakReference<GridViewRowPresenter>>();

	static DependencyProperty Reg(string name, Type type, object? defaultValue) =>
		DependencyProperty.Register(name, type, typeof(GridView), new PropertyMetadata(defaultValue));

	/// <summary>Creates a view with no columns.</summary>
	public GridView()
	{
		_columns.CollectionChanged += OnColumnsChanged;
	}

	/// <summary>Gets the columns, in display order.</summary>
	public GridViewColumnCollection Columns => _columns;

	/// <summary>Gets or sets the style of the column headers.</summary>
	public Style? ColumnHeaderContainerStyle
	{
		get => (Style?)GetValue(ColumnHeaderContainerStyleProperty);
		set => SetValue(ColumnHeaderContainerStyleProperty, value);
	}

	/// <summary>Gets or sets the template of the header content of a column that has none of its own.</summary>
	public DataTemplate? ColumnHeaderTemplate
	{
		get => (DataTemplate?)GetValue(ColumnHeaderTemplateProperty);
		set => SetValue(ColumnHeaderTemplateProperty, value);
	}

	/// <summary>Gets or sets the selector of the header template.</summary>
	public DataTemplateSelector? ColumnHeaderTemplateSelector
	{
		get => (DataTemplateSelector?)GetValue(ColumnHeaderTemplateSelectorProperty);
		set => SetValue(ColumnHeaderTemplateSelectorProperty, value);
	}

	/// <summary>Gets or sets the string format of the header content.</summary>
	public string? ColumnHeaderStringFormat
	{
		get => (string?)GetValue(ColumnHeaderStringFormatProperty);
		set => SetValue(ColumnHeaderStringFormatProperty, value);
	}

	/// <summary>Gets or sets whether the user may reorder the columns (stored only).</summary>
	public bool AllowsColumnReorder
	{
		get => (bool)GetValue(AllowsColumnReorderProperty)!;
		set => SetValue(AllowsColumnReorderProperty, value);
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property.OwnerType == typeof(GridView)) Refresh();
	}

	/// <inheritdoc/>
	internal override UIElement? CreateHeader() => Track(new GridViewRowPresenter(this, null, isHeader: true));

	/// <inheritdoc/>
	internal override UIElement? CreateRowPresenter(ListViewItem item) =>
		Track(new GridViewRowPresenter(this, item, isHeader: false));

	GridViewRowPresenter Track(GridViewRowPresenter presenter)
	{
		_presenters.RemoveAll(r => !r.TryGetTarget(out _));
		_presenters.Add(new WeakReference<GridViewRowPresenter>(presenter));
		return presenter;
	}

	void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.OldItems != null)
			foreach (GridViewColumn c in e.OldItems) c.Changed -= OnColumnChanged;
		if (e.NewItems != null)
			foreach (GridViewColumn c in e.NewItems) c.Changed += OnColumnChanged;
		if (e.Action == NotifyCollectionChangedAction.Reset)
			foreach (var c in _columns) c.Changed += OnColumnChanged;
		Refresh();
	}

	void OnColumnChanged(object? sender, EventArgs e) => Refresh();

	/// <summary>Rebuilds the header and every row cell set that this view produced.</summary>
	internal void Refresh()
	{
		foreach (var weak in _presenters.ToArray())
			if (weak.TryGetTarget(out var presenter)) presenter.Rebuild();
		RaiseChanged();
	}

	/// <summary>The size group that keeps one automatic-width column equal across the header and all rows.</summary>
	internal Gtk.SizeGroup SizeGroupFor(GridViewColumn column)
	{
		if (!_sizeGroups.TryGetValue(column, out var group))
			_sizeGroups[column] = group = new Gtk.SizeGroup(Gtk.SizeGroupMode.Horizontal);
		return group;
	}
}
