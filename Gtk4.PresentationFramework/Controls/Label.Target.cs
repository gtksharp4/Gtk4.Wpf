namespace System.Windows.Controls;

public partial class Label
{
	/// <summary>Identifies the <see cref="Target"/> dependency property.</summary>
	public static readonly DependencyProperty TargetProperty =
		DependencyProperty.Register(nameof(Target), typeof(UIElement), typeof(Label), new PropertyMetadata(null));

	/// <summary>
	/// Gets or sets the element that receives focus when the label's access key is pressed (through
	/// <see cref="Input.AccessKeyManager"/>).
	/// </summary>
	public UIElement? Target
	{
		get => (UIElement?)GetValue(TargetProperty);
		set => SetValue(TargetProperty, value);
	}
}
