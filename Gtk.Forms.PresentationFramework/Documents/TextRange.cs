namespace System.Windows.Documents;

/// <summary>
/// A range of text within a <see cref="FlowDocument"/>.
/// Phase-1 functional subset: get/set text over the whole document.
/// Full positional start/end pointers are a later phase.
/// </summary>
public class TextRange
{
	readonly Controls.RichTextBox _owner;

	internal TextRange(Controls.RichTextBox owner) => _owner = owner;

	/// <summary>
	/// Gets or sets the plain text covered by this range.
	/// Setting replaces the full document content with a single-paragraph run.
	/// </summary>
	public string Text
	{
		get => _owner.Document?.GetAllText() ?? string.Empty;
		set => _owner.SetDocumentText(value);
	}
}

/// <summary>
/// The current selection in a <see cref="Controls.RichTextBox"/>.
/// Phase-1: covers the whole document (full text selection subset).
/// </summary>
public class TextSelection : TextRange
{
	internal TextSelection(Controls.RichTextBox owner) : base(owner) { }
}