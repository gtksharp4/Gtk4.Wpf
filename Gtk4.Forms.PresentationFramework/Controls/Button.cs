using System;
using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

/// <summary>
/// A clickable button backed by a <see cref="Gtk.Button"/>. Click and command members live in
/// <see cref="ButtonBase"/>; the GTK label follows <see cref="ContentControl.Content"/> when it is a string.
/// </summary>
public partial class Button : ButtonBase
{
	static Button()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(Button),
			new PropertyMetadata(typeof(Button)));
	}

	Gtk.Button _button = null!;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_button = new Gtk.Button();
		_button.Label = (Content as string) ?? string.Empty;
		_button.Clicked += (s, e) => OnClick();
		return _button;
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == ContentProperty && _button != null && e.NewValue is string s)
			_button.Label = s;
	}
}
