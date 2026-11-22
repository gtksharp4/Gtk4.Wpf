namespace System.Windows.Controls;

public partial class Label
{
	/// <summary>Identifies the <see cref="Target"/> dependency property.</summary>
	public static readonly DependencyProperty TargetProperty =
		DependencyProperty.Register(nameof(Target), typeof(UIElement), typeof(Label), new PropertyMetadata(null));

	/// <summary>
	/// Gets or sets the element that receives focus when the label's access key is pressed.
	/// Stored only; GTK has no label mnemonic target relationship for arbitrary elements.
	/// </summary>
	public UIElement? Target
	{
		get => (UIElement?)GetValue(TargetProperty);
		set => SetValue(TargetProperty, value);
	}
}
