using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

/// <summary>
/// An exclusive-choice button backed by <see cref="Gtk.CheckButton"/>. Buttons sharing a
/// <see cref="GroupName"/> (or, with no name, the same parent) are mutually exclusive; the
/// group registry is in <c>RadioButton.Group.cs</c>.
/// </summary>
/// <remarks>
/// GTK ignores a click on an already-active radio that shares a GTK group, where WPF still raises Click.
/// </remarks>
public partial class RadioButton : ToggleButton
{
	static RadioButton()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(RadioButton),
			new PropertyMetadata(typeof(RadioButton)));
	}

	Gtk.CheckButton _button = null!;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_button = new Gtk.CheckButton { Active = IsChecked == true, UseUnderline = true, Label = AccessLabel(Content) };
		_button.Toggled += (s, e) => GtkToggled(_button.Active);
		ApplyGtkGroup();
		PresentAtCreation();
		return _button;
	}

	internal override bool GtkWidgetReady => _button != null;

	internal override void SetGtkChild(Gtk.Widget? child)
	{
		if (_button != null) _button.Child = child!;
	}

	// GTK ignores a click on an already-active grouped radio, so no Click is raised for it.
	internal override bool RaisesClickOnToggled(bool active) => active;

	internal override void SetGtkLabel(string mnemonic)
	{
		if (_button != null) _button.Label = mnemonic;
	}

	internal override void SetGtkActive(bool active)
	{
		if (_button != null && _button.Active != active)
			_button.Active = active;
	}

	// GTK flips the old member off when a peer is checked; a radio clicked off with no checked peer stays
	// checked and, when ungrouped for GTK, still raises Click as in WPF.
	internal override void GtkActiveChanged(bool active)
	{
		if (active)
			IsChecked = true;
		else if (IsChecked == true)
		{
			if (HasCheckedPeer())
				IsChecked = false;
			else
			{
				SyncGtk(true);
				if (GroupName.Length == 0) GtkClicked();
			}
		}
	}

	/// <summary>Checks this button (a radio button cannot be toggled off by the user).</summary>
	protected override void OnToggle() => IsChecked = true;

	/// <inheritdoc/>
	protected override void OnChecked(RoutedEventArgs e)
	{
		foreach (var peer in Peers())
			if (peer.IsChecked == true)
				peer.IsChecked = false;
		base.OnChecked(e);
	}
}
