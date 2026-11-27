using System.Collections.Generic;

namespace System.Windows.Controls;

public partial class ListBox
{
	// A group header is the Gtk.ListBox header of the group's first row, so rows (and the selection indexes) stay
	// one per item.
	readonly Dictionary<Gtk.ListBoxRow, UIElement> _rowHeaders = new Dictionary<Gtk.ListBoxRow, UIElement>();
	UIElement? _pendingGroupHeader;

	internal override void AddGroupHeader(UIElement header) => _pendingGroupHeader = header;

	internal override void RemoveGroupHeaders()
	{
		base.RemoveGroupHeaders();
		_pendingGroupHeader = null;
		_rowHeaders.Clear();
	}

	void AttachPendingHeader(Gtk.Widget rowWidget)
	{
		if (_pendingGroupHeader == null || rowWidget is not Gtk.ListBoxRow row) return;
		_rowHeaders[row] = _pendingGroupHeader;
		_pendingGroupHeader = null;
	}

	void UpdateRowHeader(Gtk.ListBoxRow row, Gtk.ListBoxRow? before)
	{
		if (!_rowHeaders.TryGetValue(row, out var header))
		{
			if (row.Header != null) row.Header = null;
			return;
		}
		var widget = header.Widget;
		if (row.Header == null || row.Header.Handle != widget.Handle) row.Header = widget;
	}
}
