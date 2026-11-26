using System.Collections.Generic;

namespace System.Windows.Documents;

/// <summary>The kind of one symbol run in the flattened text tree.</summary>
enum TextSymKind
{
	Start,
	End,
	Text,
	Embedded,
}

/// <summary>A contiguous symbol run: one element edge, one embedded element, or a text run.</summary>
readonly struct TextSym
{
	internal TextSym(TextSymKind kind, object node, int offset, int length)
	{
		Kind = kind;
		Node = node;
		Offset = offset;
		Length = length;
	}

	internal TextSymKind Kind { get; }

	/// <summary>The element (Block, Inline, ListItem) or, for Embedded, the hosted UIElement.</summary>
	internal object Node { get; }

	internal int Offset { get; }

	internal int Length { get; }

	internal int End => Offset + Length;
}

/// <summary>
/// The flattened symbol view of a <see cref="FlowDocument"/> as WPF counts it: each element edge is one
/// symbol, each text character one, an embedded element one. Built on demand from the live tree, so a
/// direct edit of a Run or a collection is always seen. Table elements are not walked yet.
/// </summary>
static partial class TextTree
{
	/// <summary>Number of full rebuilds (test hook).</summary>
	internal static int BuildCount;

	internal static List<TextSym> Build(FlowDocument doc)
	{
		BuildCount++;
		var list = new List<TextSym>();
		int off = 0;
		switch (doc.DetachedRoot)
		{
			case Block b: AddBlock(list, b, ref off); break;
			case Inline i: AddInline(list, i, ref off); break;
			case ListItem item: AddListItem(list, item, ref off); break;
			case null: AddBlocks(list, doc.Blocks, ref off); break;
		}
		return list;
	}

	/// <summary>The block collection the text tree starts in and the offset where its blocks begin, or null for
	/// a detached root that holds inlines.</summary>
	internal static BlockCollection? RootBlocks(FlowDocument doc, out int offset)
	{
		offset = 1;
		switch (doc.DetachedRoot)
		{
			case null: offset = 0; return doc.Blocks;
			case Section s: return s.Blocks;
			case ListItem item: return item.Blocks;
			default: return null;
		}
	}

	internal static int Total(List<TextSym> syms) => syms.Count == 0 ? 0 : syms[syms.Count - 1].End;

	static void AddBlocks(List<TextSym> l, BlockCollection blocks, ref int off)
	{
		foreach (var b in blocks)
			AddBlock(l, b, ref off);
	}

	static void Edge(List<TextSym> l, TextSymKind kind, object node, ref int off)
	{
		l.Add(new TextSym(kind, node, off, 1));
		off++;
	}

	static void AddBlock(List<TextSym> l, Block b, ref int off)
	{
		Edge(l, TextSymKind.Start, b, ref off);
		switch (b)
		{
			case Paragraph p:
				AddInlines(l, p.Inlines, ref off);
				break;
			case Section s:
				AddBlocks(l, s.Blocks, ref off);
				break;
			case List list:
				foreach (var item in list.ListItems)
					AddListItem(l, item, ref off);
				break;
			case BlockUIContainer c when c.Child != null:
				Edge(l, TextSymKind.Embedded, c.Child, ref off);
				break;
		}
		Edge(l, TextSymKind.End, b, ref off);
	}

	static void AddListItem(List<TextSym> l, ListItem item, ref int off)
	{
		Edge(l, TextSymKind.Start, item, ref off);
		AddBlocks(l, item.Blocks, ref off);
		Edge(l, TextSymKind.End, item, ref off);
	}

	static void AddInlines(List<TextSym> l, InlineCollection inlines, ref int off)
	{
		foreach (var i in inlines)
			AddInline(l, i, ref off);
	}

	static void AddInline(List<TextSym> l, Inline i, ref int off)
	{
		Edge(l, TextSymKind.Start, i, ref off);
		switch (i)
		{
			case Run r:
				if (r.Text.Length > 0)
				{
					l.Add(new TextSym(TextSymKind.Text, r, off, r.Text.Length));
					off += r.Text.Length;
				}
				break;
			case Span s:
				AddInlines(l, s.Inlines, ref off);
				break;
			case InlineUIContainer c when c.Child != null:
				Edge(l, TextSymKind.Embedded, c.Child, ref off);
				break;
		}
		Edge(l, TextSymKind.End, i, ref off);
	}

	/// <summary>Index of the symbol run containing <paramref name="offset"/>, or -1 at or past the end.</summary>
	internal static int Find(List<TextSym> l, int offset)
	{
		int lo = 0, hi = l.Count - 1;
		while (lo <= hi)
		{
			int mid = (lo + hi) / 2;
			if (offset < l[mid].Offset) hi = mid - 1;
			else if (offset >= l[mid].End) lo = mid + 1;
			else return mid;
		}
		return -1;
	}

	/// <summary>The open elements (outermost first) at <paramref name="offset"/>.</summary>
	internal static List<object> OpenElements(List<TextSym> l, int offset)
	{
		var stack = new List<object>();
		foreach (var s in l)
		{
			if (s.Offset >= offset) break;
			if (s.Kind == TextSymKind.Start) stack.Add(s.Node);
			else if (s.Kind == TextSymKind.End) stack.RemoveAt(stack.Count - 1);
		}
		return stack;
	}

	/// <summary>The kind of symbol the pointer touches in the direction, with the run length for text.</summary>
	internal static TextPointerContext Context(List<TextSym> l, int offset, LogicalDirection dir)
	{
		int i = dir == LogicalDirection.Forward ? Find(l, offset) : offset > 0 ? Find(l, offset - 1) : -1;
		if (i < 0) return TextPointerContext.None;
		switch (l[i].Kind)
		{
			case TextSymKind.Start: return TextPointerContext.ElementStart;
			case TextSymKind.End: return TextPointerContext.ElementEnd;
			case TextSymKind.Embedded: return TextPointerContext.EmbeddedElement;
			default: return TextPointerContext.Text;
		}
	}
}
