namespace System.Windows.Controls;

public partial class TextBox
{
	/// <summary>Identifies the IsReadOnlyCaretVisible property (default false).</summary>
	public static readonly DependencyProperty IsReadOnlyCaretVisibleProperty = DependencyProperty.Register(
		nameof(IsReadOnlyCaretVisible), typeof(bool), typeof(TextBox), new PropertyMetadata(false));

	/// <summary>Gets or sets whether a read-only box shows its caret (stored only; GTK shows none while read-only).</summary>
	public bool IsReadOnlyCaretVisible
	{
		get => (bool)(GetValue(IsReadOnlyCaretVisibleProperty) ?? false);
		set => SetValue(IsReadOnlyCaretVisibleProperty, value);
	}
}
