using System;
using System.Collections.Generic;

namespace System.Windows.Controls;

public partial class ComboBox
{
	/// <inheritdoc/>
	protected override void OnItemsRebuilt()
	{
		if (_dropDown != null)
		{
			RebuildStringList();
			// Re-apply any SelectedIndex that was set before the widget was created.
			var currentIdx = SelectedIndex;
			if (currentIdx >= 0 && currentIdx < Items.Count)
				ApplySelectedIndex(currentIdx);
			else
			{
				SetValue(SelectedIndexProperty, -1);
				SetValue(SelectedItemProperty, null);
			}
		}
		UpdateTextFromSelection();
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
