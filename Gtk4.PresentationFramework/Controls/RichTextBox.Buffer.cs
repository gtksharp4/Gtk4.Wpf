using System.Windows.Documents;

namespace System.Windows.Controls;

public partial class RichTextBox
{
	// ── GTK backing ──────────────────────────────────────────────────────────

	Gtk.TextView _textView = null!;
	Gtk.ScrolledWindow _scrolledWindow = null!;
	bool _updating;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_textView = new Gtk.TextView
		{
			Editable = !IsReadOnly,
			WrapMode = Gtk.WrapMode.Word,
			AcceptsTab = AcceptsTab,
		};
		_scrolledWindow = new Gtk.ScrolledWindow();
		_scrolledWindow.Child = _textView;
		ApplyScrollPolicy();

		SyncBufferFromDocument();
		// Enable undo AFTER loading initial content so the document is not itself on the undo stack.
		_textView.Buffer.EnableUndo = IsUndoEnabled;
		_textView.Buffer.Changed += OnBufferChanged;
		// Separate from the document round-trip: raise TextChanged for user edits only
		// (programmatic edits set _updating and raise it once themselves).
		_textView.Buffer.Changed += (_, _) => { if (!_updating) RaiseTextChanged(); };
		_textView.Buffer.MarkSet += OnBufferMarkSet;
		return _scrolledWindow;
	}

	// ── Buffer synchronisation ────────────────────────────────────────────────

	/// <summary>
	/// Populates the <c>Gtk.TextBuffer</c> from the current <see cref="Document"/>.
	/// Applies bold/italic/underline <c>Gtk.TextTag</c>s to matching runs.
	/// </summary>
	void SyncBufferFromDocument()
	{
		if (_textView == null) return;
		_updating = true;
		try
		{
			var buf = _textView.Buffer;
			buf.Clear();
			if (Document == null) return;

			bool firstBlock = true;
			foreach (var block in Document.Blocks)
			{
				if (!firstBlock)
					InsertAtEnd(buf, "\n");
				firstBlock = false;

				if (block is Paragraph para)
					AppendParagraph(buf, para);
				else
				{
					var sb = new System.Text.StringBuilder();
					block.AppendPlainText(sb);
					InsertAtEnd(buf, sb.ToString());
				}
			}
		}
		finally { _updating = false; }
	}

	/// <summary>
	/// Appends <paramref name="text"/> at the end of the buffer using the GTK4
	/// <c>Insert(ref TextIter, string)</c> overload (the value-iter overload is obsolete).
	/// </summary>
	static void InsertAtEnd(Gtk.TextBuffer buf, string text)
	{
		var end = buf.EndIter;
		buf.Insert(ref end, text);
	}

	static void AppendParagraph(Gtk.TextBuffer buf, Paragraph para)
	{
		foreach (var inline in para.Inlines)
			AppendInline(buf, inline);
	}

	static void AppendInline(Gtk.TextBuffer buf, Inline inline)
	{
		switch (inline)
		{
			case LineBreak _:
				InsertAtEnd(buf, "\n");
				break;
			case Run r:
				AppendRunWithTags(buf, r.Text, isBold: false, isItalic: false, isUnderline: false);
				break;
			case Bold b:
				AppendSpanWithTags(buf, b, isBold: true, isItalic: false, isUnderline: false);
				break;
			case Italic it:
				AppendSpanWithTags(buf, it, isBold: false, isItalic: true, isUnderline: false);
				break;
			case Underline u:
				AppendSpanWithTags(buf, u, isBold: false, isItalic: false, isUnderline: true);
				break;
			case Span s:
				foreach (var child in s.Inlines)
					AppendInline(buf, child);
				break;
		}
	}

	static void AppendSpanWithTags(
		Gtk.TextBuffer buf, Span span,
		bool isBold, bool isItalic, bool isUnderline)
	{
		foreach (var child in span.Inlines)
		{
			switch (child)
			{
				case Run r:
					AppendRunWithTags(buf, r.Text, isBold, isItalic, isUnderline);
					break;
				case Bold b:
					AppendSpanWithTags(buf, b, isBold: true, isItalic, isUnderline);
					break;
				case Italic it:
					AppendSpanWithTags(buf, it, isBold, isItalic: true, isUnderline);
					break;
				case Underline u:
					AppendSpanWithTags(buf, u, isBold, isItalic, isUnderline: true);
					break;
				case Span nested:
					AppendSpanWithTags(buf, nested, isBold, isItalic, isUnderline);
					break;
				default:
					AppendInline(buf, child);
					break;
			}
		}
	}

	static void AppendRunWithTags(
		Gtk.TextBuffer buf, string text,
		bool isBold, bool isItalic, bool isUnderline)
	{
		if (string.IsNullOrEmpty(text)) return;
		if (!isBold && !isItalic && !isUnderline)
		{
			InsertAtEnd(buf, text);
			return;
		}
		var start = buf.EndIter;
		int offset = start.Offset;
		InsertAtEnd(buf, text);
		var startIter = buf.GetIterAtOffset(offset);
		var endIter = buf.EndIter;

		if (isBold) buf.ApplyTag(EnsureTag(buf, "bold", t => t.Weight = Pango.Weight.Bold), startIter, endIter);
		if (isItalic) buf.ApplyTag(EnsureTag(buf, "italic", t => t.Style = Pango.Style.Italic), startIter, endIter);
		if (isUnderline) buf.ApplyTag(EnsureTag(buf, "underline", t => t.Underline = Pango.Underline.Single), startIter, endIter);
	}

	static Gtk.TextTag EnsureTag(Gtk.TextBuffer buf, string name, Action<Gtk.TextTag> configure)
	{
		var tag = buf.TagTable.Lookup(name);
		if (tag == null)
		{
			tag = new Gtk.TextTag(name);
			configure(tag);
			buf.TagTable.Add(tag);
		}
		return tag;
	}

	void OnBufferChanged(object? sender, EventArgs e)
	{
		if (_updating) return;
		// Minimal two-way: update the first paragraph's text with the buffer text.
		// Full rich round-trip editing (re-parsing formatting from tags) is a later phase.
		var plainText = _textView.Buffer.Text;
		if (Document != null)
		{
			_updating = true;
			try
			{
				if (Document.Blocks.Count == 0)
					Document.Blocks.Add(new Paragraph());
				if (Document.Blocks[0] is Paragraph p)
				{
					p.Inlines.Clear();
					if (!string.IsNullOrEmpty(plainText))
						p.Inlines.Add(new Run { Text = plainText });
				}
			}
			finally { _updating = false; }
		}
	}
}