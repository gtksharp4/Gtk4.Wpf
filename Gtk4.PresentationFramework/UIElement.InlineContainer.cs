namespace System.Windows;

public abstract partial class UIElement
{
	/// <summary>The <see cref="Documents.InlineUIContainer"/> hosting this element in a TextBlock's text, if any.</summary>
	internal Documents.InlineUIContainer? InlineContainer { get; set; }
}
