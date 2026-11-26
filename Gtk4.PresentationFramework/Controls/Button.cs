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
		_button = new Gtk.Button { UseUnderline = true };
		_button.Label = AccessLabel(Content);
		_button.Clicked += (s, e) => OnClick();
		PresentAtCreation();
		return _button;
	}

	internal override bool GtkWidgetReady => _button != null;

	internal override void SetGtkLabel(string mnemonic)
	{
		if (_button != null) _button.Label = mnemonic;
	}

	internal override void SetGtkChild(Gtk.Widget? child)
	{
		if (_button != null) _button.Child = child!;
	}
}
