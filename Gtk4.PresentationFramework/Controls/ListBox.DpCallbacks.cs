using System.Collections.Generic;

namespace System.Windows.Controls;

public partial class ListBox
{
	// ── DP callbacks (single-selection path) ─────────────────────────────────

	void ApplySelectedIndex(int idx)
	{
		if (_updating) return;

		object? newItem = (idx >= 0 && idx < Items.Count) ? Items[idx] : null;
		if (idx < 0 || idx >= Items.Count) idx = -1;
		var oldItem = SelectedItem;

		if (idx == SelectedIndex && Equals(newItem, oldItem)) return;

		SetSelectionCore(idx, newItem, oldItem);
	}

	void ApplySelectedItem(object? oldItem, object? newItem)
	{
		if (_updating) return;

		int idx = -1;
		for (int i = 0; i < Items.Count; i++)
		{
			if (Equals(Items[i], newItem)) { idx = i; break; }
		}

		if (newItem != null && idx < 0)
		{
			if (SelectedIndex == -1 && SelectedItem == null) return;
			SetSelectionCore(-1, null, oldItem);
			return;
		}

		if (idx == SelectedIndex && Equals(newItem, oldItem)) return;

		SetSelectionCore(idx, newItem, oldItem);
	}

	// ── Central single-selection mutator ─────────────────────────────────────

	/// <summary>
	/// Updates both DPs, the GTK row selection, and <see cref="SelectedItems"/>,
	/// then raises <see cref="SelectionChanged"/> exactly once.
	/// </summary>
	void SetSelectionCore(int idx, object? newItem, object? oldItem)
	{
		_updating = true;
		try
		{
			SetValue(SelectedIndexProperty, idx);
			SetValue(SelectedItemProperty, newItem);

			// Sync SelectedItems.
			_selectedItems.Clear();
			if (newItem != null) _selectedItems.Add(newItem);

			// Sync GTK visual selection.
			if (_gtkListBox != null)
			{
				if (idx < 0 || idx >= _containers.Count)
				{
					_gtkListBox.UnselectAll();
				}
				else
				{
					var row = _containers[idx].Row;
					if (row != null) _gtkListBox.SelectRow(row);
				}
			}
		}
		finally { _updating = false; }

		var added = new List<object?>();
		var removed = new List<object?>();
		if (newItem != null) added.Add(newItem);
		if (oldItem != null) removed.Add(oldItem);
		RaiseSelectionChanged(added, removed);
	}
}
