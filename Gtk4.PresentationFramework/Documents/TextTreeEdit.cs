using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Documents;

/// <summary>
/// Edits the flow-document tree by symbol offset. The collections only offer Add/Remove/Clear, so an
/// insertion rebuilds the child list through <c>Replace</c>.
/// </summary>
static partial class TextTreeEdit
{
	internal static int Size(object node)
	{
		switch (node)
		{
			case Run r: return 2 + r.Text.Length;
			case Span s: return 2 + s.Inlines.Sum(Size);
			case InlineUIContainer c: return c.Child != null ? 3 : 2;
			case Inline _: return 2;
			case Paragraph p: return 2 + p.Inlines.Sum(Size);
			case Section sec: return 2 + sec.Blocks.Sum(Size);
			case List l: return 2 + l.ListItems.Sum(Size);
			case ListItem li: return 2 + li.Blocks.Sum(Size);
			case BlockUIContainer bc: return bc.Child != null ? 3 : 2;
			default: return 2;
		}
	}

	internal static void Replace(BlockCollection col, List<Block> items)
	{
		col.Clear();
		foreach (var b in items) col.Add(b);
	}

	internal static void Replace(InlineCollection col, List<Inline> items)
	{
		col.Clear();
		foreach (var i in items) col.Add(i);
	}

	/// <summary>The block collection that holds <paramref name="block"/>, or null.</summary>
	internal static BlockCollection? FindOwner(FlowDocument doc, Block block)
	{
		if (doc.DetachedRoot is List list) return FindOwner(list, block);
		var root = TextTree.RootBlocks(doc, out _);
		return root == null ? null : FindOwner(root, block);
	}

	static BlockCollection? FindOwner(List list, Block block)
	{
		foreach (var item in list.ListItems)
		{
			var found = FindOwner(item.Blocks, block);
			if (found != null) return found;
		}
		return null;
	}

	static BlockCollection? FindOwner(BlockCollection col, Block block)
	{
		if (col.Any(x => ReferenceEquals(x, block))) return col;
		foreach (var b in col)
		{
			BlockCollection? found = null;
			if (b is Section s) found = FindOwner(s.Blocks, block);
			else if (b is List l)
				foreach (var item in l.ListItems)
					found = found ?? FindOwner(item.Blocks, block);
			if (found != null) return found;
		}
		return null;
	}

	/// <summary>
	/// Inserts text at <paramref name="offset"/>: into the Run there, else into a new Run (and a new Paragraph in a
	/// block container). Returns the offset just after the inserted text.
	/// </summary>
	/// <exception cref="InvalidOperationException">The position cannot hold text.</exception>
	internal static int InsertText(FlowDocument doc, int offset, string text, bool create = false)
	{
		var syms = doc.Symbols;
		int before = TextTree.Total(syms);
		var open = TextTree.OpenElements(syms, offset);
		object parent = open.Count == 0 ? doc : open[open.Count - 1];
		if (text.Length == 0 && !create && !(parent is Run)) return offset;
		int end;
		if (parent is Run run)
		{
			int local = offset - (StartOffset(syms, run) + 1);
			run.Text = run.Text.Insert(local, text);
			end = offset + text.Length;
		}
		else if (parent is Paragraph p)
			end = InsertRun(syms, p.Inlines, offset, text);
		else if (parent is Span s)
			end = InsertRun(syms, s.Inlines, offset, text);
		else if (parent is FlowDocument || parent is Section || parent is ListItem)
			end = InsertParagraph(doc, syms, parent, offset, text);
		else
			throw new InvalidOperationException("The position cannot hold text.");
		doc.RecordTextEdit(offset, 0, doc.SymbolTotal - before);
		return end;
	}

	internal static int StartOffset(List<TextSym> syms, object node) =>
		syms.First(s => s.Kind == TextSymKind.Start && ReferenceEquals(s.Node, node)).Offset;

	static int InsertRun(List<TextSym> syms, InlineCollection col, int offset, string text)
	{
		var items = col.ToList();
		int index = items.Count(i => StartOffset(syms, i) < offset);
		items.Insert(index, new Run { Text = text });
		Replace(col, items);
		return offset + 1 + text.Length;
	}

	static int InsertParagraph(FlowDocument doc, List<TextSym> syms, object parent, int offset, string text)
	{
		var col = parent is FlowDocument ? doc.Blocks : parent is Section sec ? sec.Blocks : ((ListItem)parent).Blocks;
		var items = col.ToList();
		int index = items.Count(b => StartOffset(syms, b) < offset);
		var para = new Paragraph();
		para.Inlines.Add(new Run { Text = text });
		items.Insert(index, para);
		Replace(col, items);
		return offset + 2 + text.Length;
	}

	/// <summary>Deletes up to <paramref name="count"/> characters of the adjacent text run; negative goes back.</summary>
	/// <returns>The number of characters deleted.</returns>
	internal static int DeleteInRun(FlowDocument doc, int offset, int count)
	{
		var syms = doc.Symbols;
		bool forward = count >= 0;
		int i = forward ? TextTree.Find(syms, offset) : offset > 0 ? TextTree.Find(syms, offset - 1) : -1;
		if (i < 0 || syms[i].Kind != TextSymKind.Text || count == 0) return 0;
		var seg = syms[i];
		var run = (Run)seg.Node;
		int local = offset - seg.Offset;
		int n = forward ? Math.Min(count, seg.End - offset) : Math.Min(-count, local);
		int from = forward ? local : local - n;
		run.Text = run.Text.Remove(from, n);
		doc.RecordTextEdit(seg.Offset + from, n, 0);
		return n;
	}
}
