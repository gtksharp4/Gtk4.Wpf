using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class ListBox
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new ListBoxAutomationPeer(this);

	// Adds or removes one item from the selection, through GTK when the rows exist.
	internal void AutomationSetSelected(object item, bool selected)
	{
		int index = Items.IndexOf(item);
		if (index < 0) return;
		if (SelectionMode != SelectionMode.Single && _gtkListBox != null && index < _containers.Count
			&& _containers[index].Row is { } row)
		{
			if (selected) _gtkListBox.SelectRow(row);
			else _gtkListBox.UnselectRow(row);
			SyncSelectedItemsFromGtk();
		}
		else if (selected) SelectedItem = item;
		else if (Equals(SelectedItem, item)) SelectedItem = null;
	}
}