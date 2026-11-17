namespace System.Windows.Controls;

/// <summary>
/// A button that alternates between checked and unchecked states, backed by
/// <see cref="Gtk.ToggleButton"/>. Two-way: GTK toggle writes back into
/// <see cref="IsChecked"/>. Derives from <see cref="ContentControl"/> (no
/// <c>ButtonBase</c> exists yet in this phase).
/// </summary>
public class ToggleButton : ContentControl
{
	/// <summary>Raised when <see cref="IsChecked"/> becomes <c>true</c>.</summary>
	public event EventHandler? Checked;

	/// <summary>Raised when <see cref="IsChecked"/> becomes <c>false</c>.</summary>
	public event EventHandler? Unchecked;

	// ── IsChecked DP ─────────────────────────────────────────────────────────

	/// <summary>Three-valued check state; <c>null</c> = indeterminate.</summary>
	public static readonly DependencyProperty IsCheckedProperty =
		DependencyProperty.Register(
			nameof(IsChecked),
			typeof(bool?),
			typeof(ToggleButton),
			new PropertyMetadata(false, OnIsCheckedChanged));

	static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var tb = (ToggleButton)d;
		var value = (e.NewValue as bool?) == true;
		if (tb._toggle != null && tb._toggle.Active != value)
			tb._toggle.Active = value;
		if (value)
			tb.Checked?.Invoke(tb, EventArgs.Empty);
		else
			tb.Unchecked?.Invoke(tb, EventArgs.Empty);
	}

	/// <summary>Gets or sets the check state.</summary>
	public bool? IsChecked
	{
		get => (bool?)GetValue(IsCheckedProperty);
		set => SetValue(IsCheckedProperty, value);
	}

	// ── GTK backing ─────────────────────────────────────────────────────────

	Gtk.ToggleButton _toggle = null!;
	bool _updating;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_toggle = new Gtk.ToggleButton();
		_toggle.Label = Content as string ?? string.Empty;
		_toggle.Active = IsChecked == true;
		_toggle.Toggled += OnGtkToggled;
		return _toggle;
	}

	/// <inheritdoc/>
	protected override Gtk.Box ContentContainer =>
		throw new InvalidOperationException("ToggleButton manages its own content.");

	/// <inheritdoc/>
	protected override Gtk.Label ContentLabel =>
		throw new InvalidOperationException("ToggleButton manages its own content.");

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == ContentProperty && _toggle != null && e.NewValue is string s)
			_toggle.Label = s;
	}

	void OnGtkToggled(object? sender, EventArgs e)
	{
		if (_updating) return;
		_updating = true;
		try { SetValue(IsCheckedProperty, _toggle.Active ? (bool?)true : false); }
		finally { _updating = false; }
	}
}