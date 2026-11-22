using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Documents;

static partial class TextTreeEdit
{
	/// <summary>
	/// Deletes the symbols in [start, end): text is cut from runs, fully covered elements are removed, and when
	/// the range crosses two paragraphs the second one is merged into the first. A document left without blocks gets
	/// one empty paragraph. The logged edit is the measured symbol difference, which is smaller than the range when
	/// the range starts outside a paragraph and the surviving paragraph keeps its edges.
	/// </summary>
	internal static void DeleteRange(FlowDocument doc, int start, int end)
	{
		if (end <= start) return;
		var syms = doc.Symbols;
		int before = TextTree.Total(syms);
		var p1 = InnerParagraph(TextTree.OpenElements(syms, start));
		var p2 = InnerParagraph(TextTree.OpenElements(syms, end));
		var root = TextTree.RootBlocks(doc, out int off);
		if (root != null) PruneBlocks(root, start, end, ref off);
		else PruneDetachedInlines(doc.DetachedRoot, start, end);
		if (p1 != null && p2 != null && !ReferenceEquals(p1, p2))
		{
			var emptied = ItemOwning(doc, p2, out var emptiedList);
			foreach (var i in p2.Inlines.ToList()) p1.Inlines.Add(i);
			p2.Inlines.Clear();
			FindOwner(doc, p2)?.Remove(p2);
			if (emptied != null) DropEmptyItems(emptiedList!, new[] { emptied });
		}
		if (doc.DetachedRoot == null && doc.Blocks.Count == 0) doc.Blocks.Add(new Paragraph());
		KeepListItem(doc.DetachedRoot as List);
		doc.RecordTextEdit(start, before - doc.SymbolTotal, 0);
	}

	/// <summary>A detached List keeps a first item holding a paragraph, as WPF does.</summary>
	static void KeepListItem(List? list)
	{
		if (list == null) return;
		if (list.ListItems.Count == 0) list.ListItems.Add(new ListItem());
		var first = list.ListItems[0];
		if (first.Blocks.Count == 0) first.Blocks.Add(new Paragraph());
	}

	/// <summary>Prunes a detached root that is not a block collection: Paragraph, Span, List or Run.</summary>
	static void PruneDetachedInlines(object? rootNode, int start, int end)
	{
		int inner = 1;
		switch (rootNode)
		{
			case Paragraph p: PruneInlines(p.Inlines, start, end, ref inner); break;
			case Span s: PruneInlines(s.Inlines, start, end, ref inner); break;
			case List l: PruneListItems(l, start, end, ref inner); break;
			case Run r:
				int from = Math.Max(start, 1) - 1, to = Math.Min(end, 1 + r.Text.Length) - 1;
				if (to > from) r.Text = r.Text.Remove(from, to - from);
				break;
		}
	}

	static Paragraph? InnerParagraph(List<object> open)
	{
		for (int i = open.Count - 1; i >= 0; i--)
			if (open[i] is Paragraph p) return p;
		return null;
	}

	static void PruneBlocks(BlockCollection col, int start, int end, ref int off)
	{
		var keep = new List<Block>();
		bool changed = false;
		foreach (var b in col)
		{
			int a = off, size = Size(b);
			off = a + size;
			if (a >= start && a + size <= end) { changed = true; continue; }
			keep.Add(b);
			if (a + size <= start || a >= end) continue;
			int inner = a + 1;
			if (b is Paragraph p) PruneInlines(p.Inlines, start, end, ref inner);
			else if (b is Section s) PruneBlocks(s.Blocks, start, end, ref inner);
			else if (b is List l) PruneListItems(l, start, end, ref inner);
		}
		if (changed) Replace(col, keep);
	}

	static void PruneListItems(List list, int start, int end, ref int inner)
	{
		var touched = new List<ListItem>();
		foreach (var item in list.ListItems)
		{
			int itemStart = inner, itemSize = Size(item);
			int body = itemStart + 1;
			if (itemStart + itemSize > start && itemStart < end)
			{
				PruneBlocks(item.Blocks, start, end, ref body);
				touched.Add(item);
			}
			inner = itemStart + itemSize;
		}
		DropEmptyItems(list, touched);
	}

	static void PruneInlines(InlineCollection col, int start, int end, ref int off)
	{
		var keep = new List<Inline>();
		bool changed = false;
		foreach (var i in col)
		{
			int a = off, size = Size(i);
			off = a + size;
			if (a >= start && a + size <= end) { changed = true; continue; }
			keep.Add(i);
			if (a + size <= start || a >= end) continue;
			if (i is Run r)
			{
				int from = Math.Max(start, a + 1) - (a + 1);
				int to = Math.Min(end, a + 1 + r.Text.Length) - (a + 1);
				if (to > from) r.Text = r.Text.Remove(from, to - from);
			}
			else if (i is Span s)
			{
				int inner = a + 1;
				PruneInlines(s.Inlines, start, end, ref inner);
			}
		}
		if (changed) Replace(col, keep);
	}
}
