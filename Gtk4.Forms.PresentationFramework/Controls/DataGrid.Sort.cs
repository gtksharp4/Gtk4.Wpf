using System.Collections.Generic;
using System.ComponentModel;

namespace System.Windows.Controls;

public partial class DataGrid
{
	// ── CanUserSortColumns DP ─────────────────────────────────────────────────

	/// <summary>
	/// Whether the user can sort columns by clicking headers. Defaults to <c>true</c>.
	/// </summary>
	public static readonly DependencyProperty CanUserSortColumnsProperty =
		DependencyProperty.Register(
			nameof(CanUserSortColumns),
			typeof(bool),
			typeof(DataGrid),
			new PropertyMetadata(true));

	/// <summary>Gets or sets whether column-header clicks trigger sorting.</summary>
	public bool CanUserSortColumns
	{
		get => (bool?)GetValue(CanUserSortColumnsProperty) ?? true;
		set => SetValue(CanUserSortColumnsProperty, value);
	}

	// ── Sort seam ─────────────────────────────────────────────────────────────

	/// <summary>
	/// Toggles the sort on <paramref name="column"/>:
	/// null → Ascending → Descending (→ Ascending…). Clears all other columns'
	/// <see cref="DataGridColumn.SortDirection"/> and re-sorts the displayed rows.
	/// </summary>
	/// <remarks>
	/// Internal so unit tests can call it without GTK header click events.
	/// </remarks>
	internal void ToggleSort(DataGridColumn column)
	{
		if (!CanUserSortColumns) return;
		if (!column.CanUserSort) return;

		// Cycle: null → Ascending → Descending → Ascending …
		var next = column.SortDirection == null
			? ListSortDirection.Ascending
			: column.SortDirection == ListSortDirection.Ascending
				? ListSortDirection.Descending
				: ListSortDirection.Ascending;

		// Clear sibling columns.
		foreach (var col in Columns)
		{
			if (!ReferenceEquals(col, column))
				col.SortDirection = null;
		}

		column.SortDirection = next;
		ApplySort(column, next);
	}

	void ApplySort(DataGridColumn column, ListSortDirection direction)
	{
		var path = ResolveSortPath(column);
		if (string.IsNullOrEmpty(path)) return;

		_rows.Clear();
		if (ItemsSource == null) return;

		var first = System.Linq.Enumerable.FirstOrDefault(System.Linq.Enumerable.OfType<object>(ItemsSource));
		path = PropertyPath.ResolveOwner(first, path);
		var sorted = DataGridSort.SortItems(ItemsSource, path!, direction);
		_rows.AddRange(sorted);

		// Rebuild GTK grid if the widget has been realized.
		if (_grid != null)
			RebuildGrid();
		ExistingAutomationPeer?.NotifyChildrenChanged();
	}

	/// <summary>
	/// Returns the effective sort path for <paramref name="column"/>:
	/// uses <see cref="DataGridColumn.SortMemberPath"/> when set, otherwise falls
	/// back to the binding path of a <see cref="DataGridTextColumn"/>.
	/// </summary>
	static string? ResolveSortPath(DataGridColumn column)
	{
		if (!string.IsNullOrEmpty(column.SortMemberPath))
			return column.SortMemberPath;
		if (column is DataGridTextColumn tc)
			return tc.Binding?.PathString;
		return null;
	}

	/// <summary>Attaches a sort-click handler to a header label for a realized column.</summary>
	void AttachHeaderClick(Gtk.Label header, DataGridColumn column)
	{
		var click = new Gtk.GestureClick();
		click.Pressed += (s, e) => ToggleSort(column);
		header.AddController(click);
	}
}
