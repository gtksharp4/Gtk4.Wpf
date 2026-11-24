using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>
/// A single page in a <see cref="TabControl"/>, backed by its content widget.
/// The <see cref="HeaderedContentControl.Header"/> appears in the tab strip.
/// </summary>
[ContentProperty("Content")]
public partial class TabItem : HeaderedContentControl
{
	/// <summary>Returns the header as a string (used to label the tab).</summary>
	internal string HeaderText => HeaderTextProjection;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		// TabItem's widget is a plain box that hosts its content.
		var box = new Gtk.Box(Gtk.Orientation.Vertical, 0);
		_tabBox = box;
		RefreshPresentation();
		return box;
	}

	Gtk.Box? _tabBox;

	internal override bool PresentationReady => _tabBox != null;

	internal override void HostElement(Gtk.Widget widget) => _tabBox!.Add(widget);

	internal override void UnhostElement(Gtk.Widget widget) => _tabBox!.Remove(widget);

	internal override void ShowText(string text)
	{
		if (_textLabel != null) _tabBox!.Remove(_textLabel);
		_textLabel = text.Length == 0 ? null : new Gtk.Label(text) { Xalign = 0f };
		if (_textLabel != null) _tabBox!.Add(_textLabel);
	}

	Gtk.Label? _textLabel;
}