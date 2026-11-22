using System.Collections.Generic;

namespace System.Windows.Documents;

public sealed partial class TextPointer
{
	/// <summary>The number of text characters next to this pointer in <paramref name="direction"/>.</summary>
	public int GetTextRunLength(LogicalDirection direction)
	{
		var syms = _doc.Symbols;
		int off = Offset;
		return TextRun(syms, off, direction, out var seg) ? RunSpan(seg, off, direction) : 0;
	}

	/// <summary>The text next to this pointer in <paramref name="direction"/>, up to the next non-text symbol.</summary>
	public string GetTextInRun(LogicalDirection direction)
	{
		var syms = _doc.Symbols;
		int off = Offset;
		if (!TextRun(syms, off, direction, out var seg)) return string.Empty;
		var text = ((Run)seg.Node).Text;
		int local = off - seg.Offset;
		return direction == LogicalDirection.Forward ? text.Substring(local) : text.Substring(0, local);
	}

	/// <summary>Copies up to <paramref name="count"/> characters of the adjacent run into the buffer.</summary>
	/// <returns>The number of characters copied.</returns>
	public int GetTextInRun(LogicalDirection direction, char[] textBuffer, int startIndex, int count)
	{
		if (textBuffer == null) throw new ArgumentNullException(nameof(textBuffer));
		if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex));
		if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
		if (startIndex > textBuffer.Length) throw new ArgumentException("startIndex is past the end of the buffer.");
		if (startIndex + count > textBuffer.Length) throw new ArgumentException("The buffer is too small.");
		var text = GetTextInRun(direction);
		int n = Math.Min(count, text.Length);
		int from = direction == LogicalDirection.Forward ? 0 : text.Length - n;
		text.CopyTo(from, textBuffer, startIndex, n);
		return n;
	}

	static bool TextRun(List<TextSym> syms, int off, LogicalDirection dir, out TextSym seg)
	{
		int i = dir == LogicalDirection.Forward ? TextTree.Find(syms, off) : off > 0 ? TextTree.Find(syms, off - 1) : -1;
		seg = i < 0 ? default : syms[i];
		return i >= 0 && seg.Kind == TextSymKind.Text;
	}

	static int RunSpan(TextSym seg, int off, LogicalDirection dir) =>
		dir == LogicalDirection.Forward ? seg.End - off : off - seg.Offset;

	/// <summary>The pointer past the next symbol (a whole text run counts as one), or null at the edge.</summary>
	public TextPointer? GetNextContextPosition(LogicalDirection direction)
	{
		var syms = _doc.Symbols;
		int off = Offset;
		var ctx = TextTree.Context(syms, off, direction);
		if (ctx == TextPointerContext.None) return null;
		int step = 1;
		if (ctx == TextPointerContext.Text && TextRun(syms, off, direction, out var seg))
			step = RunSpan(seg, off, direction);
		return new TextPointer(_doc, direction == LogicalDirection.Forward ? off + step : off - step, _direction);
	}

	/// <summary>The element or embedded UIElement next to this pointer, or null next to text or the edge.</summary>
	/// <remarks>Typed object: the flow elements are not DependencyObjects in this layer.</remarks>
	public object? GetAdjacentElement(LogicalDirection direction)
	{
		var syms = _doc.Symbols;
		int off = Offset;
		int i = direction == LogicalDirection.Forward ? TextTree.Find(syms, off) : off > 0 ? TextTree.Find(syms, off - 1) : -1;
		return i < 0 || syms[i].Kind == TextSymKind.Text ? null : syms[i].Node;
	}

	/// <summary>True when a caret can be placed here.</summary>
	public bool IsAtInsertionPosition
	{
		get
		{
			int off = Offset;
			foreach (var b in InsertionBoundaries(_doc.Symbols))
				if (b.Offset == off) return b.Valid;
			return true; // inside a text run
		}
	}

	/// <summary>The nearest insertion position in <paramref name="direction"/>, else the other way, else this.</summary>
	public TextPointer GetInsertionPosition(LogicalDirection direction)
	{
		if (IsAtInsertionPosition) return new TextPointer(_doc, Offset, direction);
		var found = FindInsertion(direction == LogicalDirection.Forward)
			?? FindInsertion(direction != LogicalDirection.Forward);
		return new TextPointer(_doc, found ?? Offset, direction);
	}

	/// <summary>The next insertion position after this one in <paramref name="direction"/>, or null.</summary>
	public TextPointer? GetNextInsertionPosition(LogicalDirection direction)
	{
		int off = Offset;
		bool forward = direction == LogicalDirection.Forward;
		int? next = FindInsertion(forward, off + (forward ? 1 : -1));
		return next == null ? null : new TextPointer(_doc, next.Value, direction);
	}

	int? FindInsertion(bool forward, int? from = null)
	{
		var syms = _doc.Symbols;
		int off = from ?? Offset;
		int total = TextTree.Total(syms);
		if (off < 0 || off > total) return null;
		var boundaries = InsertionBoundaries(syms);
		int? best = null;
		foreach (var b in boundaries)
			if (b.Valid && (forward ? b.Offset >= off : b.Offset <= off)
				&& (best == null || (forward ? b.Offset < best : b.Offset > best)))
				best = b.Offset;
		// Positions strictly inside a text run are insertion positions too.
		foreach (var s in syms)
		{
			if (s.Kind != TextSymKind.Text) continue;
			int cand = forward ? Math.Max(off, s.Offset + 1) : Math.Min(off, s.End - 1);
			if (cand > s.Offset && cand < s.End && (best == null || (forward ? cand < best : cand > best)))
				best = cand;
		}
		return best;
	}

	/// <summary>Every structural boundary with whether a caret may rest there.</summary>
	static IEnumerable<(int Offset, bool Valid)> InsertionBoundaries(List<TextSym> syms)
	{
		var stack = new List<object>();
		for (int i = 0; i <= syms.Count; i++)
		{
			int offset = i < syms.Count ? syms[i].Offset : TextTree.Total(syms);
			object? parent = stack.Count == 0 ? null : stack[stack.Count - 1];
			TextSym? prev = i > 0 ? syms[i - 1] : (TextSym?)null;
			TextSym? next = i < syms.Count ? syms[i] : (TextSym?)null;
			yield return (offset, IsCaretSpot(parent, prev, next));
			if (i == syms.Count) break;
			if (syms[i].Kind == TextSymKind.Start) stack.Add(syms[i].Node);
			else if (syms[i].Kind == TextSymKind.End) stack.RemoveAt(stack.Count - 1);
		}
	}

	static bool IsCaretSpot(object? parent, TextSym? prev, TextSym? next)
	{
		if (parent is Run) return true;
		if (!(parent is Paragraph) && !(parent is Span)) return false;
		if (prev is TextSym p && p.Kind == TextSymKind.End && p.Node is Run) return false;
		if (next is TextSym n && n.Kind == TextSymKind.Start && n.Node is Run) return false;
		return true;
	}
}
