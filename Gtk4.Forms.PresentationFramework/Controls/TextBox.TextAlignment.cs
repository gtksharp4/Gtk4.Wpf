namespace System.Windows.Controls;

public partial class TextBox
{
	/// <summary>Identifies the TextAlignment property (default Left; inherits).</summary>
	public static readonly DependencyProperty TextAlignmentProperty =
		TextBlock.TextAlignmentProperty.AddOwner(typeof(TextBox),
			new FrameworkPropertyMetadata(TextAlignment.Left,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Inherits,
				(d, e) => ((TextBox)d).ApplyTextAlignment()));

	/// <summary>Gets or sets the horizontal alignment of the text.</summary>
	public TextAlignment TextAlignment
	{
		get => (TextAlignment)GetValue(TextAlignmentProperty)!;
		set => SetValue(TextAlignmentProperty, value);
	}

	// Justify has no GtkEntry equivalent (it aligns left there); the multiline view fills.
	void ApplyTextAlignment()
	{
		var (justify, xalign) = TextBlock.MapTextAlignment(TextAlignment);
		if (_entry != null) _entry.Alignment = xalign;
		if (_textView != null) _textView.Justification = justify;
	}
}
