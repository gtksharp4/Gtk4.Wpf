namespace System.Windows.Controls;

/// <summary>
/// An item in a <see cref="ComboBox"/>. The GTK drop-down shows items as text, so
/// <see cref="IsHighlighted"/> is store-only, <c>IsEnabled=false</c> does not disable the row
/// (GtkDropDown cannot), and a <see cref="UIElement"/> content shows as its text.
/// </summary>
public class ComboBoxItem : ListBoxItem
{
	static readonly DependencyPropertyKey _isHighlightedKey =
		DependencyProperty.RegisterReadOnly(nameof(IsHighlighted), typeof(bool), typeof(ComboBoxItem),
			new FrameworkPropertyMetadata(false));

	/// <summary>Identifies <see cref="IsHighlighted"/>.</summary>
	public static readonly DependencyProperty IsHighlightedProperty = _isHighlightedKey.DependencyProperty;

	/// <summary>Whether the item is highlighted (store-only on GTK).</summary>
	public bool IsHighlighted => (bool)GetValue(IsHighlightedProperty)!;
}
