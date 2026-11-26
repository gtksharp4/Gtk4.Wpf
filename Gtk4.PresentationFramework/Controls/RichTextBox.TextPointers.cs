using System.Windows.Documents;

namespace System.Windows.Controls;

public partial class RichTextBox
{
	TextPointer? _caret;

	/// <summary>
	/// The caret position in the document. Stored: it does not follow the GTK caret, and setting it does not
	/// move the GTK caret.
	/// </summary>
	public TextPointer CaretPosition
	{
		get => _caret ?? (Document ??= new FlowDocument()).ContentStart;
		set
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (Document == null || !ReferenceEquals(value.Document, Document))
				throw new ArgumentException("The position is not in this control's document.", nameof(value));
			_caret = value;
			RaiseSelectionChanged();
		}
	}

	// A new RichTextBox holds an empty document with one empty paragraph, as in WPF.
	static FlowDocument NewEmptyDocument()
	{
		var doc = new FlowDocument();
		doc.Blocks.Add(new Paragraph());
		return doc;
	}

	// A TextRange edit changes the Document tree behind the GTK buffer's back; re-sync the buffer.
	void AttachDocumentEdits(FlowDocument? oldDocument, FlowDocument? newDocument)
	{
		if (oldDocument != null) oldDocument.ContentEdited -= OnDocumentContentEdited;
		if (newDocument != null) newDocument.ContentEdited += OnDocumentContentEdited;
		_caret = null;
	}

	void OnDocumentContentEdited()
	{
		SyncBufferFromDocument();
		RaiseTextChanged();
	}
}
