using System.Text;

namespace System.Windows.Documents;

/// <summary>
/// A range of content in a <see cref="FlowDocument"/>, between two <see cref="TextPointer"/>s. A range created
/// for a <see cref="Controls.RichTextBox"/> selection without an explicit range covers the whole document.
/// </summary>
public partial class TextRange
{
	readonly Controls.RichTextBox? _owner;
	TextPointer? _start;
	TextPointer? _end;

	/// <summary>Creates a range between two positions of the same document; the ends are ordered.</summary>
	/// <exception cref="ArgumentNullException">A position is null.</exception>
	/// <exception cref="ArgumentException">The positions belong to different documents.</exception>
	public TextRange(TextPointer position1, TextPointer position2)
	{
		if (position1 == null) throw new ArgumentNullException(nameof(position1));
		if (position2 == null) throw new ArgumentNullException(nameof(position2));
		Set(position1, position2);
	}

	internal TextRange(Controls.RichTextBox owner) => _owner = owner;

	void Set(TextPointer p1, TextPointer p2)
	{
		if (!p1.IsInSameDocument(p2))
			throw new ArgumentException("The positions are not in the same document.", nameof(p2));
		bool ordered = p1.CompareTo(p2) <= 0;
		var a = ordered ? p1 : p2;
		var b = ordered ? p2 : p1;
		_start = new TextPointer(a.Document, a.Offset, LogicalDirection.Backward);
		_end = new TextPointer(b.Document, b.Offset, LogicalDirection.Forward);
	}

	bool WholeOwner => _owner != null && _start == null;

	/// <summary>The start of the range.</summary>
	public TextPointer Start => WholeOwner ? OwnerDocument().ContentStart : _start!;

	/// <summary>The end of the range.</summary>
	public TextPointer End => WholeOwner ? OwnerDocument().ContentEnd : _end!;

	FlowDocument OwnerDocument() => _owner!.Document ?? (_owner.Document = new FlowDocument());

	/// <summary>Raised when the range is moved by <see cref="Select"/> or its text is replaced.</summary>
	public event EventHandler? Changed;

	/// <summary>Moves the range to span the two positions.</summary>
	public void Select(TextPointer position1, TextPointer position2)
	{
		if (position1 == null) throw new ArgumentNullException(nameof(position1));
		if (position2 == null) throw new ArgumentNullException(nameof(position2));
		Set(position1, position2);
		Changed?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>True when the range holds no text or embedded element.</summary>
	public bool IsEmpty
	{
		get
		{
			if (WholeOwner && _owner!.Document == null) return true;
			int s = Start.Offset, e = End.Offset;
			foreach (var seg in TextTree.Build(Start.Document))
				if (s < e && seg.Kind != TextSymKind.Start && seg.Kind != TextSymKind.End && seg.Offset < e && seg.End > s)
					return false;
			return true;
		}
	}

	/// <summary>True when <paramref name="textPointer"/> lies within the range, ends included.</summary>
	/// <exception cref="ArgumentException">The pointer belongs to another document.</exception>
	public bool Contains(TextPointer textPointer)
	{
		if (textPointer == null) throw new ArgumentNullException(nameof(textPointer));
		if (WholeOwner && _owner!.Document == null) return false;
		if (!Start.IsInSameDocument(textPointer))
			throw new ArgumentException("The pointer is not in this range's document.", nameof(textPointer));
		return Start.Offset <= textPointer.Offset && textPointer.Offset <= End.Offset;
	}

	/// <summary>
	/// Gets or sets the plain text of the range. Paragraph ends and line breaks read as CRLF, as in WPF; setting
	/// replaces only the range content and keeps the range around the new text.
	/// </summary>
	public string Text
	{
		get
		{
			if (WholeOwner) return _owner!.Document?.GetAllText() ?? string.Empty;
			return Read(Start.Document, Start.Offset, End.Offset);
		}
		set
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (WholeOwner) { _owner!.SetDocumentText(value); return; }
			var doc = Start.Document;
			int s = Start.Offset, e = End.Offset;
			doc.EditBatch(() => TextTreeEdit.ReplaceText(doc, s, e, value));
			Changed?.Invoke(this, EventArgs.Empty);
		}
	}

	static string Read(FlowDocument doc, int s, int e)
	{
		var sb = new StringBuilder();
		foreach (var seg in TextTree.Build(doc))
		{
			if (seg.End <= s || seg.Offset >= e) continue;
			if (seg.Kind == TextSymKind.Text)
			{
				int from = Math.Max(s, seg.Offset) - seg.Offset;
				int to = Math.Min(e, seg.End) - seg.Offset;
				sb.Append(((Run)seg.Node).Text, from, to - from);
			}
			else if (seg.Kind == TextSymKind.End && seg.Node is Paragraph) sb.Append("\r\n");
			else if (seg.Kind == TextSymKind.Start && seg.Node is LineBreak) sb.Append("\r\n");
		}
		return sb.ToString();
	}
}

/// <summary>
/// The current selection of a <see cref="Controls.RichTextBox"/>. Covers the whole document until
/// <see cref="TextRange.Select"/> narrows it; it does not follow the GTK caret.
/// </summary>
public class TextSelection : TextRange
{
	internal TextSelection(Controls.RichTextBox owner) : base(owner) { }
}
