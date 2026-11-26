using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Controls;

public partial class ListBox
{
	// ── SelectionMode DP ──────────────────────────────────────────────────────

	/// <summary>
	/// Gets or sets the selection behaviour of the <see cref="ListBox"/>.
	/// <para>
	/// <see cref="Controls.SelectionMode.Multiple"/> and
	/// <see cref="Controls.SelectionMode.Extended"/> both map to
	/// <see cref="Gtk.SelectionMode.Multiple"/>. The WPF keyboard-modifier
	/// distinction (toggle vs range) is not reproduced — GTK 4 handles
	/// multi-selection uniformly.
	/// </para>
	/// </summary>
	public static readonly DependencyProperty SelectionModeProperty =
		DependencyProperty.Register(
			nameof(SelectionMode),
			typeof(SelectionMode),
			typeof(ListBox),
			new PropertyMetadata(SelectionMode.Single, OnSelectionModeChanged));

	static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ListBox)d).ApplySelectionMode((SelectionMode)(e.NewValue ?? SelectionMode.Single));
	}

	/// <summary>Gets or sets the selection mode.</summary>
	public SelectionMode SelectionMode
	{
		get => (SelectionMode)(GetValue(SelectionModeProperty) ?? SelectionMode.Single);
		set => SetValue(SelectionModeProperty, value);
	}

	// ── SelectedItems (reflects current multi-selection) ─────────────────────

	/// <summary>
	/// The currently selected items. In Single mode this contains at most one
	/// entry. In Multiple/Extended mode it reflects all selected items. The
	/// list is managed by <see cref="ListBox"/>; callers should not modify it
	/// directly (WPF exposes this as a read-only-ish <see cref="IList"/>).
	/// </summary>
	public IList SelectedItems => _selectedItems;

	readonly List<object?> _selectedItems = new List<object?>();

	// ── SelectionMode → Gtk.SelectionMode mapping ─────────────────────────────

	/// <summary>
	/// Maps a WPF <see cref="Controls.SelectionMode"/> to the equivalent
	/// <see cref="Gtk.SelectionMode"/>.
	/// </summary>
	/// <remarks>
	/// <see cref="Controls.SelectionMode.Multiple"/> and
	/// <see cref="Controls.SelectionMode.Extended"/> both map to
	/// <see cref="Gtk.SelectionMode.Multiple"/>; GTK 4 does not distinguish
	/// them at the input level.
	/// </remarks>
	internal static Gtk.SelectionMode ToGtkSelectionMode(SelectionMode mode)
	{
		switch (mode)
		{
			case SelectionMode.Multiple:
			case SelectionMode.Extended:
				return Gtk.SelectionMode.Multiple;
			default:
				return Gtk.SelectionMode.Single;
		}
	}

	// ── Applying the mode ─────────────────────────────────────────────────────

	void ApplySelectionMode(SelectionMode mode)
	{
		if (_gtkListBox != null)
			_gtkListBox.SelectionMode = ToGtkSelectionMode(mode);

		// Switching to Single: keep only the primary selection.
		if (mode == SelectionMode.Single && _selectedItems.Count > 1)
		{
			var keep = _selectedItems[0];
			var removed = new List<object?>(_selectedItems.GetRange(1, _selectedItems.Count - 1));
			_selectedItems.RemoveRange(1, _selectedItems.Count - 1);
			RaiseSelectionChanged(new List<object?>(), removed);
		}
	}

	// ── Programmatic multi-select API ─────────────────────────────────────────

	/// <summary>
	/// Selects all items. Only effective when
	/// <see cref="SelectionMode"/> is <see cref="Controls.SelectionMode.Multiple"/>
	/// or <see cref="Controls.SelectionMode.Extended"/>.
	/// </summary>
	public void SelectAll()
	{
		if (SelectionMode == SelectionMode.Single) return;
		if (_gtkListBox != null)
			_gtkListBox.SelectAll();
		SyncSelectedItemsFromGtk();
	}

	/// <summary>Deselects all items.</summary>
	public void UnselectAll()
	{
		if (_gtkListBox != null)
		{
			_gtkListBox.UnselectAll();
		}
		else
		{
			// headless: clear managed model
			if (_selectedItems.Count == 0) return;
			var removed = new List<object?>(_selectedItems);
			_selectedItems.Clear();
			_updating = true;
			try
			{
				SetValue(SelectedIndexProperty, -1);
				SetValue(SelectedItemProperty, null);
			}
			finally { _updating = false; }
			RaiseSelectionChanged(new List<object?>(), removed);
		}
	}

	// ── Sync SelectedItems from GTK (after GTK selection changes) ────────────

	/// <summary>
	/// Rebuilds <see cref="_selectedItems"/> from the GTK selection and raises
	/// <see cref="SelectionChanged"/> with the computed delta.
	/// </summary>
	internal void SyncSelectedItemsFromGtk()
	{
		if (_gtkListBox == null) return;

		var gtkRows = _gtkListBox.SelectedRows ?? Array.Empty<Gtk.ListBoxRow>();
		var newSelection = new List<object?>();
		foreach (var gtkRow in gtkRows)
		{
			for (int i = 0; i < _containers.Count; i++)
			{
				if (ReferenceEquals(_containers[i].Row, gtkRow) && i < Items.Count)
				{
					newSelection.Add(Items[i]);
					break;
				}
			}
		}

		ComputeDeltaAndApply(newSelection);
	}

	/// <summary>
	/// Applies a new selection set, computes added/removed deltas, updates all
	/// managed state, and raises <see cref="SelectionChanged"/> exactly once.
	/// </summary>
	void ComputeDeltaAndApply(List<object?> newSelection)
	{
		// Compute delta.
		var added = new List<object?>();
		var removed = new List<object?>();

		foreach (var item in newSelection)
			if (!_selectedItems.Contains(item))
				added.Add(item);

		foreach (var item in _selectedItems)
			if (!newSelection.Contains(item))
				removed.Add(item);

		if (added.Count == 0 && removed.Count == 0) return;

		_selectedItems.Clear();
		_selectedItems.AddRange(newSelection);

		var primaryItem = _selectedItems.Count > 0 ? _selectedItems[0] : null;
		var primaryIdx = primaryItem != null ? Items.IndexOf(primaryItem) : -1;

		_updating = true;
		try
		{
			SetValue(SelectedIndexProperty, primaryIdx);
			SetValue(SelectedItemProperty, primaryItem);
		}
		finally { _updating = false; }

		RaiseSelectionChanged(added, removed);
	}
}
