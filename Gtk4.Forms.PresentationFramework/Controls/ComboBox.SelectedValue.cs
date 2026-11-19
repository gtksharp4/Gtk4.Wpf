namespace System.Windows.Controls;

public partial class ComboBox
{
	// ── SelectedValuePath DP ──────────────────────────────────────────────────

	/// <summary>
	/// The property path used to derive <see cref="SelectedValue"/> from the
	/// selected item.  Empty string (the default) means <see cref="SelectedValue"/>
	/// is the selected item itself.
	/// </summary>
	public static readonly DependencyProperty SelectedValuePathProperty =
		DependencyProperty.Register(
			nameof(SelectedValuePath),
			typeof(string),
			typeof(ComboBox),
			new PropertyMetadata(string.Empty, OnSelectedValuePathChanged));

	static void OnSelectedValuePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ComboBox)d).RefreshSelectedValue();
	}

	/// <summary>Gets or sets the property path used to compute <see cref="SelectedValue"/>.</summary>
	public string SelectedValuePath
	{
		get => (string?)GetValue(SelectedValuePathProperty) ?? string.Empty;
		set => SetValue(SelectedValuePathProperty, value);
	}

	// ── SelectedValue DP ──────────────────────────────────────────────────────

	/// <summary>
	/// The value of the <see cref="SelectedValuePath"/> property on the current
	/// <see cref="SelectedItem"/>.  When set, selects the item whose value-path
	/// value equals the assigned value; clears selection if no item matches.
	/// </summary>
	public static readonly DependencyProperty SelectedValueProperty =
		DependencyProperty.Register(
			nameof(SelectedValue),
			typeof(object),
			typeof(ComboBox),
			new FrameworkPropertyMetadata(null,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValueChanged));

	static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ComboBox)d).ApplySelectedValue(e.NewValue);
	}

	/// <summary>Gets or sets the selected value.</summary>
	public object? SelectedValue
	{
		get => GetValue(SelectedValueProperty);
		set => SetValue(SelectedValueProperty, value);
	}

	// ── Sync helpers ──────────────────────────────────────────────────────────

	/// <summary>
	/// Pushes the derived value-path result of the current <see cref="SelectedItem"/>
	/// into <see cref="SelectedValue"/>.
	/// </summary>
	internal void RefreshSelectedValue()
	{
		if (_updatingValue) return;
		var derived = SelectorValueSupport.GetValue(SelectedItem, SelectedValuePath);
		_updatingValue = true;
		try { SetValue(SelectedValueProperty, derived); }
		finally { _updatingValue = false; }
	}

	/// <summary>
	/// Called when <see cref="SelectedValue"/> is assigned programmatically.
	/// Finds the matching item and selects it; clears selection if none match.
	/// </summary>
	void ApplySelectedValue(object? value)
	{
		if (_updatingValue) return;
		_updatingValue = true;
		try
		{
			var match = SelectorValueSupport.FindByValue((System.Collections.IList)Items, SelectedValuePath, value);
			SelectedItem = match;
		}
		finally { _updatingValue = false; }
	}

	/// <summary>
	/// Re-entrant guard for SelectedValue↔SelectedItem cross-sync.
	/// Separate from <see cref="_updating"/> (which guards GTK↔DP sync).
	/// </summary>
	bool _updatingValue;
}
