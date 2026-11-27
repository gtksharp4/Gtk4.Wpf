namespace System.Windows.Controls;

/// <summary>
/// Text and editable mode. <see cref="IsEditable"/> only takes effect when set before the
/// widget is created (a Box with an Entry and the DropDown); toggling later is store-only.
/// <see cref="StaysOpenOnEdit"/> is store-only and typed text is not prefix auto-completed.
/// </summary>
public partial class ComboBox
{
	/// <summary>Identifies <see cref="IsEditable"/>.</summary>
	public static readonly DependencyProperty IsEditableProperty =
		DependencyProperty.Register(nameof(IsEditable), typeof(bool), typeof(ComboBox),
			new FrameworkPropertyMetadata(false));

	/// <summary>Identifies <see cref="IsReadOnly"/> (shared with <see cref="TextBox.IsReadOnlyProperty"/>).</summary>
	public static readonly DependencyProperty IsReadOnlyProperty =
		TextBox.IsReadOnlyProperty.AddOwner(typeof(ComboBox),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, OnIsReadOnlyChanged));

	static ComboBox() =>
		IsTextSearchEnabledProperty.OverrideMetadata(typeof(ComboBox), new FrameworkPropertyMetadata(true));

	/// <summary>Identifies <see cref="StaysOpenOnEdit"/>.</summary>
	public static readonly DependencyProperty StaysOpenOnEditProperty =
		DependencyProperty.Register(nameof(StaysOpenOnEdit), typeof(bool), typeof(ComboBox),
			new FrameworkPropertyMetadata(false));

	/// <summary>Identifies <see cref="Text"/>.</summary>
	public static readonly DependencyProperty TextProperty =
		DependencyProperty.Register(nameof(Text), typeof(string), typeof(ComboBox),
			new FrameworkPropertyMetadata(string.Empty,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
				OnTextChanged, null, false, Data.UpdateSourceTrigger.LostFocus));

	/// <summary>Gets or sets whether the text box is editable (honoured at widget creation).</summary>
	public bool IsEditable
	{
		get => (bool)GetValue(IsEditableProperty)!;
		set => SetValue(IsEditableProperty, value);
	}

	/// <summary>Gets or sets whether the editable text is read-only.</summary>
	public bool IsReadOnly
	{
		get => (bool)GetValue(IsReadOnlyProperty)!;
		set => SetValue(IsReadOnlyProperty, value);
	}

	/// <summary>Gets or sets whether the drop-down stays open while editing (store-only).</summary>
	public bool StaysOpenOnEdit
	{
		get => (bool)GetValue(StaysOpenOnEditProperty)!;
		set => SetValue(StaysOpenOnEditProperty, value);
	}

	/// <summary>The selected item text, or the typed text when editable.</summary>
	public string Text
	{
		get => (string?)GetValue(TextProperty) ?? string.Empty;
		set => SetValue(TextProperty, value);
	}

	TextBox? _editableTextBox;
	bool _updatingText;
	bool _syncingEntry;

	/// <summary>The editable text box (WPF template part PART_EditableTextBox); null when not editable.</summary>
	internal TextBox? EditableTextBoxSite => _editableTextBox;

	Gtk.Widget CreateEditableHost()
	{
		_editableTextBox = new TextBox { Text = Text, IsReadOnly = IsReadOnly };
		_editableTextBox.LogicalParent = this;
		_editableTextBox.TextChanged += OnEntryChanged;
		var box = new Gtk.Box(Gtk.Orientation.Horizontal, 0);
		box.Add(_editableTextBox.Widget);
		box.Add(_dropDown);
		return box;
	}

	static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var cb = (ComboBox)d;
		if (cb._editableTextBox != null) cb._editableTextBox.IsReadOnly = (bool)e.NewValue!;
	}

	void OnEntryChanged(object? sender, TextChangedEventArgs e)
	{
		if (_syncingEntry || _editableTextBox == null) return;
		SetValue(TextProperty, _editableTextBox.Text);
	}

	static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var cb = (ComboBox)d;
		var text = (string?)e.NewValue ?? string.Empty;
		if (cb._editableTextBox != null && cb._editableTextBox.Text != text)
		{
			cb._syncingEntry = true;
			try { cb._editableTextBox.Text = text; }
			finally { cb._syncingEntry = false; }
		}
		if (cb._updatingText || !cb.IsTextSearchEnabled) return;
		cb._updatingText = true;
		try { cb.SelectMatchingItem(text); }
		finally { cb._updatingText = false; }
	}

	/// <summary>Selects the first exact match, or clears the selection when there is none.</summary>
	void SelectMatchingItem(string text)
	{
		var idx = ComboBoxItemText.IndexOfExact(ItemTexts(), text, IsTextSearchCaseSensitive);
		if (idx == SelectedIndex) return;
		SetValue(SelectedIndexProperty, idx);
		SetValue(SelectedItemProperty, idx >= 0 ? Items[idx] : null);
	}

	void OnSelectionChangedForText()
	{
		UpdateSelectionBox();
		UpdateTextFromSelection();
	}

	/// <summary>Pushes the selected item text into <see cref="Text"/> (not while Text drives selection).</summary>
	void UpdateTextFromSelection()
	{
		if (_updatingText) return;
		var item = SelectedItem;
		if (item == null && IsEditable) return;
		_updatingText = true;
		try { SetValue(TextProperty, item == null ? string.Empty : ComboBoxItemText.GetText(item, DisplayMemberPath)); }
		finally { _updatingText = false; }
	}
}
