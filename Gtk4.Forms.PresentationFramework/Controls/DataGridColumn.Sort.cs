using System.ComponentModel;

namespace System.Windows.Controls;

public abstract partial class DataGridColumn
{
	// ── CanUserSort DP ────────────────────────────────────────────────────────

	/// <summary>Whether the user can sort by this column. Defaults to <c>true</c>.</summary>
	public static readonly DependencyProperty CanUserSortProperty =
		DependencyProperty.Register(
			nameof(CanUserSort),
			typeof(bool),
			typeof(DataGridColumn),
			new PropertyMetadata(true));

	/// <summary>Gets or sets whether the user can sort by this column.</summary>
	public bool CanUserSort
	{
		get => (bool?)GetValue(CanUserSortProperty) ?? true;
		set => SetValue(CanUserSortProperty, value);
	}

	// ── SortMemberPath DP ────────────────────────────────────────────────────

	/// <summary>
	/// The property path used as the sort key. When empty the column's binding
	/// path is used as a fallback.
	/// </summary>
	public static readonly DependencyProperty SortMemberPathProperty =
		DependencyProperty.Register(
			nameof(SortMemberPath),
			typeof(string),
			typeof(DataGridColumn),
			new PropertyMetadata(null));

	/// <summary>Gets or sets the sort member path.</summary>
	public string? SortMemberPath
	{
		get => (string?)GetValue(SortMemberPathProperty);
		set => SetValue(SortMemberPathProperty, value);
	}

	// ── SortDirection DP ─────────────────────────────────────────────────────

	/// <summary>
	/// Current sort direction of this column; <c>null</c> means unsorted.
	/// </summary>
	public static readonly DependencyProperty SortDirectionProperty =
		DependencyProperty.Register(
			nameof(SortDirection),
			typeof(ListSortDirection?),
			typeof(DataGridColumn),
			new PropertyMetadata(null));

	/// <summary>Gets or sets the current sort direction (null = unsorted).</summary>
	public ListSortDirection? SortDirection
	{
		get => (ListSortDirection?)GetValue(SortDirectionProperty);
		set => SetValue(SortDirectionProperty, value);
	}
}
