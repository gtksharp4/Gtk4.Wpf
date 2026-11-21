namespace System.Windows.Documents;

public partial class Section
{
	/// <summary>Identifies the HasTrailingParagraphBreakOnPaste property (default true).</summary>
	public static readonly DependencyProperty HasTrailingParagraphBreakOnPasteProperty =
		DependencyProperty.Register(nameof(HasTrailingParagraphBreakOnPaste), typeof(bool), typeof(Section),
			new FrameworkPropertyMetadata(true));

	/// <summary>Gets or sets whether pasting the section adds a trailing paragraph break (stored).</summary>
	public bool HasTrailingParagraphBreakOnPaste
	{
		get => (bool)GetValue(HasTrailingParagraphBreakOnPasteProperty)!;
		set => SetValue(HasTrailingParagraphBreakOnPasteProperty, value);
	}
}
