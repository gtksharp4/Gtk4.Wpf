namespace System.Windows;

public abstract partial class FrameworkElement
{
	/// <summary>Identifies the Tag property (default null).</summary>
	public static readonly DependencyProperty TagProperty = DependencyProperty.Register(
		nameof(Tag), typeof(object), typeof(FrameworkElement), new FrameworkPropertyMetadata((object?)null));

	/// <summary>Gets or sets an arbitrary object tied to this element.</summary>
	public object? Tag
	{
		get => GetValue(TagProperty);
		set => SetValue(TagProperty, value);
	}
}
