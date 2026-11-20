using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

/// <summary>
/// A check control backed by <see cref="Gtk.CheckButton"/>. State, events and three-state
/// behaviour come from <see cref="ToggleButton"/>.
/// </summary>
public class CheckBox : ToggleButton
{
	static CheckBox()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckBox),
			new PropertyMetadata(typeof(CheckBox)));
	}

	Gtk.CheckButton _check = null!;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_check = new Gtk.CheckButton { Active = IsChecked == true };
		_check.Toggled += (s, e) => GtkToggled(_check.Active);
		_check.Activate += (s, e) => GtkClicked();
		return _check;
	}

	internal override void SetGtkActive(bool active)
	{
		if (_check != null && _check.Active != active)
			_check.Active = active;
	}
}
