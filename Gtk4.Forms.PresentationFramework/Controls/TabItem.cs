using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>
/// A single page in a <see cref="TabControl"/>, backed by its content widget.
/// The <see cref="HeaderedContentControl.Header"/> appears in the tab strip.
/// </summary>
[ContentProperty("Content")]
public class TabItem : HeaderedContentControl
{
	/// <summary>Returns the header as a string (used to label the tab).</summary>
	internal string HeaderText => Header as string ?? Header?.ToString() ?? string.Empty;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		// TabItem's widget is a plain box that hosts its content.
		var box = new Gtk.Box(Gtk.Orientation.Vertical, 0);
		if (Content is UIElement child)
			box.Add(child.Widget);
		return box;
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == ContentProperty && IsWidgetCreated)
		{
			var box = (Gtk.Box)Widget;
			if (e.OldValue is UIElement old)
				box.Remove(old.Widget);
			if (e.NewValue is UIElement newEl)
				box.Add(newEl.Widget);
		}
	}
}