using System.Collections.Generic;

namespace System.Windows.Controls;

public partial class DataGrid
{
	readonly List<(UIElement Element, object? Content)> _headerElements = new List<(UIElement, object?)>();

	// Releases the header elements of the previous grid build.
	void ReleaseHeaderElements()
	{
		foreach (var (element, content) in _headerElements)
			ContentResolution.Release(element, content);
		_headerElements.Clear();
	}

	// An element, template or inline header is hosted as is; any other header is a text label.
	Gtk.Widget CreateHeaderWidget(DataGridColumn column)
	{
		var header = column.Header;
		var element = ContentResolution.ResolveStructured(
			this, header, column.HeaderTemplate, column.HeaderTemplateSelector, out _);
		if (element != null)
		{
			_headerElements.Add((element, header));
			return element.Widget;
		}
		return new Gtk.Label(ContentResolution.ToText(header, column.HeaderStringFormat));
	}
}
