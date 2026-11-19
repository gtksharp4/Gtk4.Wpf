namespace System.Windows.Controls;

/// <summary>
/// Read-only selection-box DPs. They track the selection but nothing renders from them:
/// GtkDropDown draws its own selected-text button (so <see cref="IsSelectionBoxHighlighted"/> is store-only).
/// </summary>
public partial class ComboBox
{
	static readonly DependencyPropertyKey _selectionBoxItemKey =
		DependencyProperty.RegisterReadOnly(nameof(SelectionBoxItem), typeof(object), typeof(ComboBox),
			new FrameworkPropertyMetadata(string.Empty));

	static readonly DependencyPropertyKey _selectionBoxItemTemplateKey =
		DependencyProperty.RegisterReadOnly(nameof(SelectionBoxItemTemplate), typeof(DataTemplate), typeof(ComboBox),
			new FrameworkPropertyMetadata(null));

	static readonly DependencyPropertyKey _selectionBoxItemStringFormatKey =
		DependencyProperty.RegisterReadOnly(nameof(SelectionBoxItemStringFormat), typeof(string), typeof(ComboBox),
			new FrameworkPropertyMetadata(null));

	static readonly DependencyPropertyKey _isSelectionBoxHighlightedKey =
		DependencyProperty.RegisterReadOnly(nameof(IsSelectionBoxHighlighted), typeof(bool), typeof(ComboBox),
			new FrameworkPropertyMetadata(false));

	/// <summary>Identifies <see cref="SelectionBoxItem"/>.</summary>
	public static readonly DependencyProperty SelectionBoxItemProperty = _selectionBoxItemKey.DependencyProperty;

	/// <summary>Identifies <see cref="SelectionBoxItemTemplate"/>.</summary>
	public static readonly DependencyProperty SelectionBoxItemTemplateProperty =
		_selectionBoxItemTemplateKey.DependencyProperty;

	/// <summary>Identifies <see cref="SelectionBoxItemStringFormat"/>.</summary>
	public static readonly DependencyProperty SelectionBoxItemStringFormatProperty =
		_selectionBoxItemStringFormatKey.DependencyProperty;

	/// <summary>Identifies <see cref="IsSelectionBoxHighlighted"/>.</summary>
	public static readonly DependencyProperty IsSelectionBoxHighlightedProperty =
		_isSelectionBoxHighlightedKey.DependencyProperty;

	/// <summary>The content shown in the selection box ("" when nothing is selected).</summary>
	public object SelectionBoxItem => GetValue(SelectionBoxItemProperty) ?? string.Empty;

	/// <summary>The template of the selection-box item.</summary>
	public DataTemplate? SelectionBoxItemTemplate => (DataTemplate?)GetValue(SelectionBoxItemTemplateProperty);

	/// <summary>The string format of the selection-box item (never set: no <c>ItemStringFormat</c> yet).</summary>
	public string? SelectionBoxItemStringFormat => (string?)GetValue(SelectionBoxItemStringFormatProperty);

	/// <summary>Whether the selection box is highlighted (store-only).</summary>
	public bool IsSelectionBoxHighlighted => (bool)GetValue(IsSelectionBoxHighlightedProperty)!;

	void UpdateSelectionBox()
	{
		var item = SelectedItem;
		object shown = item is ComboBoxItem cbi ? cbi.Content ?? string.Empty : item ?? string.Empty;
		SetValue(_selectionBoxItemKey, shown);
		SetValue(_selectionBoxItemTemplateKey, item == null || item is ComboBoxItem ? null : ItemTemplate);
	}
}
