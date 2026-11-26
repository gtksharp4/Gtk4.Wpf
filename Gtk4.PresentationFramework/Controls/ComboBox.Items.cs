using System;
using System.Collections.Generic;

namespace System.Windows.Controls;

public partial class ComboBox
{
	readonly List<UIElement> _parentedItems = new List<UIElement>();

	/// <inheritdoc/>
	internal override bool RebuildsOnInlineItemsChange => true;

	/// <inheritdoc/>
	protected override void OnItemsRebuilt()
	{
		// Items is already mutated; the selection DPs still hold the pre-change values.
		var prevItem = SelectedItem;
		var prevIdx = SelectedIndex;
		UpdateItemParents();
		RebuildStringList();
		ReconcileSelection(prevItem, prevIdx);
		UpdateTextFromSelection();
	}

	/// <summary>The selection follows its item; an index set before the items stays pending until they exist.</summary>
	void ReconcileSelection(object? prevItem, int prevIdx)
	{
		int newIdx;
		if (prevItem != null)
		{
			newIdx = prevIdx >= 0 && prevIdx < Items.Count && Equals(Items[prevIdx], prevItem)
				? prevIdx
				: Items.IndexOf(prevItem);
		}
		else
			newIdx = prevIdx;
		var newItem = newIdx >= 0 && newIdx < Items.Count ? Items[newIdx] : null;

		var wasUpdating = _updating;
		_updating = true;
		try
		{
			ApplySelectedIndex(newIdx);
			SetValue(SelectedIndexProperty, newIdx);
			SetValue(SelectedItemProperty, newItem);
		}
		finally { _updating = wasUpdating; }
		if (!Equals(prevItem, newItem))
			NotifySelectionChanged(prevItem, newItem);
	}

	/// <summary>
	/// Brings SelectedItem in line with an in-range SelectedIndex (a fresh model fires no GTK notify).
	/// </summary>
	void SyncSelectedItemToIndex()
	{
		var idx = SelectedIndex;
		if (idx < 0 || idx >= Items.Count) return;
		var item = Items[idx];
		var old = SelectedItem;
		if (Equals(old, item)) return;
		_updating = true;
		try { SetValue(SelectedItemProperty, item); }
		finally { _updating = false; }
		NotifySelectionChanged(old, item);
	}

	/// <summary>Items that are their own containers get this control as logical parent.</summary>
	void UpdateItemParents()
	{
		foreach (var old in _parentedItems)
			if (ReferenceEquals(old.LogicalParent, this)) old.LogicalParent = null;
		_parentedItems.Clear();
		foreach (var item in Items)
		{
			if (item is UIElement ui)
			{
				ui.LogicalParent = this;
				_parentedItems.Add(ui);
			}
		}
	}

	List<string> ItemTexts()
	{
		var path = DisplayMemberPath;
		var texts = new List<string>(Items.Count);
		foreach (var item in Items)
			texts.Add(ComboBoxItemText.GetText(item, path));
		return texts;
	}

	void RebuildStringList()
	{
		if (_dropDown == null) return;
		// Suppress GTK "selected" notifications while we swap the model.
		_updating = true;
		try
		{
			_stringList = new Gtk.StringList(ItemTexts().ToArray());
			_dropDown.Model = _stringList;
		}
		finally { _updating = false; }
	}

	void ApplySelectedIndex(int index)
	{
		if (_dropDown == null) return;
		if (index < 0 || index >= Items.Count)
		{
			_dropDown.Selected = uint.MaxValue; // GTK_INVALID_LIST_POSITION
			return;
		}
		if (_dropDown.Selected != (uint)index)
			_dropDown.Selected = (uint)index;
	}
}
