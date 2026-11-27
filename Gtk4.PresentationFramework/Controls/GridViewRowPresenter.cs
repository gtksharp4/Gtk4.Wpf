using System.Collections.Generic;
using System.Windows.Data;

namespace System.Windows.Controls;

/// <summary>
/// Lays the cells of one <see cref="GridView"/> row side by side, one per column: a <see cref="TextBlock"/> bound
/// by <see cref="GridViewColumn.DisplayMemberBinding"/>, else a <see cref="ContentPresenter"/> showing the row item
/// through <see cref="GridViewColumn.CellTemplate"/>. The same presenter, in header mode, lays out the
/// <see cref="GridViewColumnHeader"/>s. Backed by a horizontal <see cref="Gtk.Box"/>.
/// </summary>
public class GridViewRowPresenter : FrameworkElement
{
	readonly GridView? _view;
	readonly ListViewItem? _owner;
	readonly bool _isHeader;
	readonly List<UIElement> _cells = new List<UIElement>();
	readonly List<(Gtk.SizeGroup Group, Gtk.Widget Widget)> _grouped = new List<(Gtk.SizeGroup, Gtk.Widget)>();
	Gtk.Box? _box;
	GridViewColumnCollection? _columns;

	/// <summary>Creates a presenter; give it <see cref="Columns"/> and <see cref="Content"/>.</summary>
	public GridViewRowPresenter()
	{
	}

	internal GridViewRowPresenter(GridView view, ListViewItem? owner, bool isHeader)
	{
		_view = view;
		_owner = owner;
		_isHeader = isHeader;
		_columns = view.Columns;
		if (owner != null) LogicalParent = owner;
	}

	/// <summary>Gets or sets the columns that decide the cells.</summary>
	public GridViewColumnCollection? Columns
	{
		get => _columns;
		set
		{
			_columns = value;
			Rebuild();
		}
	}

	/// <summary>The row item the cells show: the content of the owning <see cref="ListViewItem"/>.</summary>
	public object? Content => _owner?.Content;

	/// <inheritdoc/>
	internal override bool IsWidgetCreated => _box != null;

	/// <inheritdoc/>
	internal override IEnumerable<UIElement> VisualChildren => _cells;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_box = new Gtk.Box(Gtk.Orientation.Horizontal, 0);
		Build();
		return _box;
	}

	/// <summary>Discards the cells and builds them again from the current columns.</summary>
	internal void Rebuild()
	{
		if (_box == null) return;
		Clear();
		Build();
	}

	void Clear()
	{
		foreach (var (group, widget) in _grouped) group.RemoveWidget(widget);
		_grouped.Clear();
		foreach (var cell in _cells)
		{
			_box!.Remove(cell.Widget);
			cell.LogicalParent = null;
		}
		_cells.Clear();
	}

	void Build()
	{
		if (_columns == null) return;
		var content = Content;
		if (!_isHeader && content != null && content is not UIElement) DataContext = content;
		foreach (var column in _columns)
		{
			var cell = _isHeader ? CreateHeaderCell(column) : CreateRowCell(column, content);
			cell.LogicalParent = this;
			if (cell is FrameworkElement fe) fe.Width = column.Width;
			_cells.Add(cell);
			var widget = cell.Widget;
			_box!.Append(widget);
			if (_isHeader) column.HeaderWidget = widget;
			if (_view != null && double.IsNaN(column.Width))
			{
				var group = _view.SizeGroupFor(column);
				group.AddWidget(widget);
				_grouped.Add((group, widget));
			}
		}
	}

	UIElement CreateHeaderCell(GridViewColumn column)
	{
		var view = _view;
		var header = new GridViewColumnHeader
		{
			Column = column,
			Content = column.Header,
			ContentTemplate = column.HeaderTemplate ?? view?.ColumnHeaderTemplate,
			ContentTemplateSelector = column.HeaderTemplateSelector ?? view?.ColumnHeaderTemplateSelector,
			ContentStringFormat = column.HeaderStringFormat ?? view?.ColumnHeaderStringFormat,
		};
		if (view?.ColumnHeaderContainerStyle != null) header.Style = view.ColumnHeaderContainerStyle;
		return header;
	}

	static UIElement CreateRowCell(GridViewColumn column, object? content)
	{
		if (column.DisplayMemberBinding != null)
		{
			var text = new TextBlock();
			BindingOperations.SetBinding(text, TextBlock.TextProperty, column.DisplayMemberBinding);
			return text;
		}
		return new ContentPresenter
		{
			Content = content,
			ContentTemplate = column.CellTemplate,
			ContentTemplateSelector = column.CellTemplateSelector,
		};
	}
}
