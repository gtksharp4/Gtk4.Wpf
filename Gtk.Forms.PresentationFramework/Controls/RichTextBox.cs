using System.Windows.Documents;

namespace System.Windows.Controls
{
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
	public class RichTextBox : Control
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
				WrapMode = Gtk.WrapMode.Word
			};
			_scrolledWindow = new Gtk.ScrolledWindow();
			_scrolledWindow.Child = _textView;

			SyncBufferFromDocument();
			_textView.Buffer.Changed += OnBufferChanged;
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
					{
						buf.Insert(buf.EndIter, "\n");
					}
					firstBlock = false;

					if (block is Paragraph para)
						AppendParagraph(buf, para);
					else
					{
						// Fallback: plain text for non-paragraph blocks.
						var sb = new System.Text.StringBuilder();
						block.AppendPlainText(sb);
						buf.Insert(buf.EndIter, sb.ToString());
					}
				}
			}
			finally { _updating = false; }
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
					buf.Insert(buf.EndIter, "\n");
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
				buf.Insert(buf.EndIter, text);
				return;
			}
			var start = buf.EndIter;
			int offset = start.Offset;
			buf.Insert(buf.EndIter, text);
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

		// ── AppendText ────────────────────────────────────────────────────────────

		/// <summary>
		/// Appends plain text to the document as a new <see cref="Run"/> in a new
		/// <see cref="Paragraph"/>.  Also appends to the GTK buffer if the widget exists.
		/// </summary>
		public void AppendText(string text)
		{
			if (Document == null)
				Document = new FlowDocument();

			var para = new Paragraph();
			para.Inlines.Add(new Run { Text = text });
			Document.Blocks.Add(para);

			if (_textView != null)
			{
				_updating = true;
				try
				{
					if (_textView.Buffer.CharCount > 0)
						_textView.Buffer.Insert(_textView.Buffer.EndIter, "\n");
					_textView.Buffer.Insert(_textView.Buffer.EndIter, text);
				}
				finally { _updating = false; }
			}
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
		}

		/// <summary>Initialises the selection lazily.</summary>
		public RichTextBox()
		{
			_selection = new TextSelection(this);
		}
	}
}
