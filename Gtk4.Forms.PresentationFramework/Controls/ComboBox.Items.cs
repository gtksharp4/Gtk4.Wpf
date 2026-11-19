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
		UpdateItemParents();
		if (_dropDown != null)
		{
			RebuildStringList();
			// Re-apply (or clear) the selection: GTK picks item 0 on a new model.
			var currentIdx = SelectedIndex;
			if (currentIdx >= 0 && currentIdx < Items.Count)
				ApplySelectedIndex(currentIdx);
			else
			{
				ApplySelectedIndex(-1);
				SetValue(SelectedIndexProperty, -1);
				SetValue(SelectedItemProperty, null);
			}
		}
		UpdateTextFromSelection();
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
