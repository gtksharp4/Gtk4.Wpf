using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Documents;

static partial class TextTreeEdit
{
	/// <summary>
	/// Replaces [start, end) with parsed paragraphs. A range over the whole document swaps the blocks; otherwise
	/// the first paragraph joins the paragraph at the range start and the last one the text after the range.
	/// </summary>
	internal static void ReplaceContent(FlowDocument doc, int start, int end, List<Paragraph> blocks)
	{
		int total = doc.SymbolTotal;
		if (doc.DetachedRoot == null && start <= 0 && end >= total)
		{
			doc.Blocks.Clear();
			foreach (var b in blocks) doc.Blocks.Add(b);
			if (doc.Blocks.Count == 0) doc.Blocks.Add(new Paragraph());
			doc.RecordTextEdit(0, total, doc.SymbolTotal);
			return;
		}
		DeleteRange(doc, start, end);
		if (blocks.Count == 0) return;
		int at = new TextPointer(doc, start, LogicalDirection.Backward).GetInsertionPosition(LogicalDirection.Forward).Offset;
		int before = doc.SymbolTotal;
		var open = TextTree.OpenElements(doc.Symbols, at);
		var para = InnerParagraph(open);
		if (para == null) InsertBlocks(doc, open, at, blocks);
		else
		{
			var tail = SplitParagraph(doc, para, at);
			foreach (var i in blocks[0].Inlines.ToList()) para.Inlines.Add(i);
			var last = blocks.Count == 1 ? para : blocks[blocks.Count - 1];
			foreach (var i in tail.Inlines.ToList()) last.Inlines.Add(i);
			if (blocks.Count > 1)
			{
				var owner = FindOwner(doc, para)!;
				var items = owner.ToList();
				items.InsertRange(items.IndexOf(para) + 1, blocks.Skip(1));
				Replace(owner, items);
			}
		}
		doc.RecordTextEdit(at, 0, doc.SymbolTotal - before);
	}

	static void InsertBlocks(FlowDocument doc, List<object> open, int at, List<Paragraph> blocks)
	{
		object parent = open.Count == 0 ? doc : open[open.Count - 1];
		var col = parent is FlowDocument ? doc.Blocks : parent is Section sec ? sec.Blocks
			: parent is ListItem li ? li.Blocks : throw new InvalidOperationException("The position cannot hold paragraphs.");
		var syms = doc.Symbols;
		var items = col.ToList();
		items.InsertRange(items.Count(b => StartOffset(syms, b) < at), blocks);
		Replace(col, items);
	}
}
