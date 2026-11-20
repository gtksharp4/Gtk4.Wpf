using System;

namespace System.Windows.Controls.Primitives;

/// <summary>
/// A button that alternates between checked, unchecked and (when <see cref="IsThreeState"/>)
/// indeterminate. Backed by <see cref="Gtk.ToggleButton"/>; the GTK toggle writes back into
/// <see cref="IsChecked"/>. Routed events and DPs live in <c>ToggleButton.Events.cs</c>.
/// </summary>
public partial class ToggleButton : ButtonBase
{
	static ToggleButton()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleButton),
			new PropertyMetadata(typeof(ToggleButton)));
	}

	Gtk.ToggleButton _toggle = null!;
	bool _syncing;
	bool _gtkClick;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_toggle = new Gtk.ToggleButton();
		_toggle.Label = Content as string ?? string.Empty;
		_toggle.Active = IsChecked == true;
		_toggle.Toggled += (s, e) => GtkToggled(_toggle.Active);
		_toggle.Clicked += (s, e) => GtkClicked();
		return _toggle;
	}

	/// <summary>Pushes the checked state to the GTK widget (subclasses with another widget override).</summary>
	internal virtual void SetGtkActive(bool active)
	{
		if (_toggle != null && _toggle.Active != active)
			_toggle.Active = active;
	}

	/// <summary>Reacts to the GTK widget's active state changing on its own.</summary>
	internal virtual void GtkActiveChanged(bool active)
	{
		if (active != (IsChecked == true))
			OnToggle();
	}

	internal void GtkToggled(bool active)
	{
		if (!_syncing)
			GtkActiveChanged(active);
	}

	// The toggle already happened in GTK (Toggled precedes Clicked), so OnClick must not toggle again.
	internal void GtkClicked()
	{
		_gtkClick = true;
		try { OnClick(); }
		finally { _gtkClick = false; }
	}

	internal void SyncGtk(bool active)
	{
		if (_syncing) return;
		_syncing = true;
		try { SetGtkActive(active); }
		finally { _syncing = false; }
	}

	/// <summary>Moves to the next state (WPF cycle), then raises <see cref="ButtonBase.Click"/>.</summary>
	protected override void OnClick()
	{
		if (!_gtkClick)
			OnToggle();
		base.OnClick();
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == ContentProperty && _toggle != null && e.NewValue is string s)
			_toggle.Label = s;
	}
}
