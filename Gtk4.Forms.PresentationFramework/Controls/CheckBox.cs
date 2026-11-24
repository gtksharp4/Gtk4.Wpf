using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

/// <summary>
/// A check control backed by <see cref="Gtk.CheckButton"/>. State, events and three-state
/// behaviour come from <see cref="ToggleButton"/>.
/// </summary>
public partial class CheckBox : ToggleButton
{
	static CheckBox()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckBox),
			new PropertyMetadata(typeof(CheckBox)));
	}

	Gtk.CheckButton _check = null!;

	/// <inheritdoc/>
	protected override System.Windows.Automation.Peers.AutomationPeer? OnCreateAutomationPeer() =>
		new System.Windows.Automation.Peers.CheckBoxAutomationPeer(this);

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_check = new Gtk.CheckButton { Active = IsChecked == true, UseUnderline = true, Label = AccessLabel(Content) };
		_check.Toggled += (s, e) => GtkToggled(_check.Active);
		PresentAtCreation();
		return _check;
	}

	internal override bool GtkWidgetReady => _check != null;

	internal override void SetGtkChild(Gtk.Widget? child)
	{
		if (_check != null) _check.Child = child!;
	}

	internal override bool RaisesClickOnToggled(bool active) => true;

	internal override void SetGtkLabel(string mnemonic)
	{
		if (_check != null) _check.Label = mnemonic;
	}

	internal override void SetGtkActive(bool active)
	{
		if (_check != null && _check.Active != active)
			_check.Active = active;
	}
}
