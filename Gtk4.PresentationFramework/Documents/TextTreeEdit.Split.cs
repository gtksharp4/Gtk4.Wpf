using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Documents;

static partial class TextTreeEdit
{
	/// <summary>
	/// Replaces [start, end) with <paramref name="text"/>. Line breaks in the text start new paragraphs.
	/// The text lands in the Run at the (normalised) insertion position, else in a new Run.
	/// </summary>
	internal static void ReplaceText(FlowDocument doc, int start, int end, string text)
	{
		var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
		DeleteRange(doc, start, end);
		int at = new TextPointer(doc, start, LogicalDirection.Backward)
			.GetInsertionPosition(LogicalDirection.Forward).Offset;
		int after = InsertText(doc, at, lines[0], create: lines.Length > 1);
		if (lines.Length == 1) return;

		var para = new TextPointer(doc, after, LogicalDirection.Forward).Paragraph!;
		int before = doc.SymbolTotal;
		var tail = SplitParagraph(doc, para, after);
		var created = new List<Block>();
		for (int i = 1; i < lines.Length - 1; i++)
			created.Add(NewParagraph(lines[i]));
		if (lines[lines.Length - 1].Length > 0 || tail.Inlines.Count == 0)
		{
			var inl = tail.Inlines.ToList();
			inl.Insert(0, new Run { Text = lines[lines.Length - 1] });
			Replace(tail.Inlines, inl);
		}
		created.Add(tail);
		var owner = FindOwner(doc, para)!;
		var items = owner.ToList();
		items.InsertRange(items.IndexOf(para) + 1, created);
		Replace(owner, items);
		doc.RecordTextEdit(after, 0, doc.SymbolTotal - before);
	}

	static Paragraph NewParagraph(string text)
	{
		var p = new Paragraph();
		p.Inlines.Add(new Run { Text = text });
		return p;
	}

	/// <summary>Moves everything after <paramref name="at"/> into a new paragraph (not yet attached).</summary>
	static Paragraph SplitParagraph(FlowDocument doc, Paragraph para, int at)
	{
		int off = StartOffset(doc.Symbols, para) + 1;
		var tail = new Paragraph();
		CopyFormat(para, tail);
		foreach (var i in SplitInlines(para.Inlines, ref off, at)) tail.Inlines.Add(i);
		return tail;
	}

	static List<Inline> SplitInlines(InlineCollection col, ref int off, int at)
	{
		var head = new List<Inline>();
		var tail = new List<Inline>();
		foreach (var i in col)
		{
			int a = off, size = Size(i);
			off = a + size;
			if (tail.Count > 0 || a >= at) { tail.Add(i); continue; }
			if (a + size <= at || !(i is Run || i is Span)) { head.Add(i); continue; }
			if (i is Run r)
			{
				int local = at - (a + 1);
				var rest = new Run { Text = r.Text.Substring(local) };
				CopyFormat(r, rest);
				r.Text = r.Text.Substring(0, local);
				head.Add(r);
				tail.Add(rest);
			}
			else
			{
				var s = (Span)i;
				int inner = a + 1;
				var shell = CloneShell(s);
				foreach (var t in SplitInlines(s.Inlines, ref inner, at)) shell.Inlines.Add(t);
				head.Add(s);
				tail.Add(shell);
			}
		}
		Replace(col, head);
		return tail;
	}

	internal static Span CloneShell(Span s)
	{
		Span copy = s is Bold ? new Bold() : s is Italic ? new Italic() : s is Underline ? new Underline()
			: s is Hyperlink h ? new Hyperlink { NavigateUri = h.NavigateUri } : new Span();
		CopyFormat(s, copy);
		return copy;
	}

	internal static void CopyFormat(TextElement from, TextElement to)
	{
		CopyLocal(from, to, TextElement.FontFamilyProperty);
		CopyLocal(from, to, TextElement.FontWeightProperty);
		CopyLocal(from, to, TextElement.FontStyleProperty);
		CopyLocal(from, to, TextElement.FontStretchProperty);
		CopyLocal(from, to, TextElement.ForegroundProperty);
		CopyLocal(from, to, TextElement.FontSizeProperty);
		to.TextDecorations = from.TextDecorations;
	}

	static void CopyLocal(TextElement from, TextElement to, DependencyProperty dp)
	{
		if (from.HasNonInheritedValue(dp)) to.SetValue(dp, from.GetValue(dp));
	}
}
