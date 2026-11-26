using System.Windows.Documents;

namespace System.Windows.Controls;

/// <summary>
/// A multi-line rich-text editor backed by a <see cref="Gtk.TextView"/> inside a
/// <see cref="Gtk.ScrolledWindow"/>.
///
/// <para><b>Fidelity scope (Phase 1).</b> The <see cref="Document"/> is rendered as
/// text into the <c>Gtk.TextBuffer</c>: bold/italic/underline runs are tagged with
/// <c>Gtk.TextTag</c>s. Two-way editing is minimal: GTK edits update a plain-text
/// view of the document (the first paragraph's content); full rich round-trip
/// editing (re-parsing formatted buffer back into <see cref="Paragraph"/>s) is a
/// later phase.</para>
///
/// <para><b>IsReadOnly</b> maps to <c>Gtk.TextView.Editable = !value</c>.</para>
/// </summary>
public partial class RichTextBox : Primitives.TextBoxBase
{
	// ── Document DP ──────────────────────────────────────────────────────────

	/// <summary>The flow document displayed and edited by this control.</summary>
	public static readonly DependencyProperty DocumentProperty =
		DependencyProperty.Register(
			nameof(Document),
			typeof(FlowDocument),
			typeof(RichTextBox),
			new PropertyMetadata(null, OnDocumentChanged));

	static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var rtb = (RichTextBox)d;
		rtb._selection = new TextSelection(rtb);
		rtb.AttachDocumentEdits((FlowDocument?)e.OldValue, (FlowDocument?)e.NewValue);
		rtb.SyncBufferFromDocument();
	}

	/// <summary>Gets or sets the flow document.</summary>
	public FlowDocument? Document
	{
		get => (FlowDocument?)GetValue(DocumentProperty);
		set => SetValue(DocumentProperty, value);
	}

	// ── IsReadOnly DP ─────────────────────────────────────────────────────────

	/// <summary>Whether the user can edit the content.</summary>
	public static readonly DependencyProperty IsReadOnlyProperty =
		DependencyProperty.Register(
			nameof(IsReadOnly),
			typeof(bool),
			typeof(RichTextBox),
			new PropertyMetadata(false, OnIsReadOnlyChanged));

	static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var rtb = (RichTextBox)d;
		if (rtb._textView != null)
			rtb._textView.Editable = !(bool)(e.NewValue ?? false);
	}

	/// <summary>Gets or sets whether the control is read-only.</summary>
	public bool IsReadOnly
	{
		get => (bool)(GetValue(IsReadOnlyProperty) ?? false);
		set => SetValue(IsReadOnlyProperty, value);
	}

	// ── Selection ─────────────────────────────────────────────────────────────

	TextSelection _selection;

	/// <summary>
	/// The current text selection (Phase-1 subset: covers the whole document text).
	/// </summary>
	public TextSelection Selection => _selection;

	// ── AppendText ────────────────────────────────────────────────────────────

	/// <summary>
	/// Appends plain text to the document as a new <see cref="Run"/> in a new
	/// <see cref="Paragraph"/>.  Also appends to the GTK buffer if the widget exists.
	/// </summary>
	public void AppendText(string text)
	{
		if (Document == null)
			Document = new FlowDocument();

		// The initial empty paragraph takes the first text instead of leaving a blank line before it.
		Paragraph para;
		if (Document.Blocks.Count == 1 && Document.Blocks[0] is Paragraph empty && empty.Inlines.Count == 0)
			para = empty;
		else
		{
			para = new Paragraph();
			Document.Blocks.Add(para);
		}
		para.Inlines.Add(new Run { Text = text });

		if (_textView != null)
		{
			_updating = true;
			try
			{
				if (_textView.Buffer.CharCount > 0)
					InsertAtEnd(_textView.Buffer, "\n");
				InsertAtEnd(_textView.Buffer, text);
			}
			finally { _updating = false; }
		}

		RaiseTextChanged();
	}

	// ── Internal helper used by TextRange/TextSelection ───────────────────────

	/// <summary>
	/// Replaces the document with a single paragraph containing <paramref name="text"/>.
	/// </summary>
	internal void SetDocumentText(string text)
	{
		if (Document == null)
			Document = new FlowDocument();

		Document.Blocks.Clear();
		var para = new Paragraph();
		if (!string.IsNullOrEmpty(text))
			para.Inlines.Add(new Run { Text = text });
		Document.Blocks.Add(para);
		SyncBufferFromDocument();
		RaiseTextChanged();
	}

	/// <summary>Initialises the selection lazily.</summary>
	public RichTextBox()
	{
		_selection = new TextSelection(this);
		Document = NewEmptyDocument();
	}
}