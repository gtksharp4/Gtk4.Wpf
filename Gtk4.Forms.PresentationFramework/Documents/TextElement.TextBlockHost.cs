namespace System.Windows.Documents;

/// <summary>
/// The private container a <see cref="Controls.TextBlock"/> mirrors its inlines into so text pointers can span
/// them. The inlines keep the TextBlock as their parent; the host only holds them for the text tree.
/// </summary>
sealed class TextBlockHost : Span
{
	internal TextBlockHost(DependencyObject owner) => SetLogicalParent(owner);

	/// <summary>The owning TextBlock.</summary>
	internal DependencyObject Owner => Parent!;

	/// <summary>The text container of the mirrored content.</summary>
	internal FlowDocument Container => ContentStart.Document;
}
