using System.Collections.Generic;

namespace System.Windows.Documents;

/// <summary>
/// Copies the content of a range into detached paragraphs, clipping text at the range edges. Lists and
/// sections are flattened to paragraphs; embedded UI elements are dropped. The serializers walk this copy.
/// </summary>
static class TextRangeExtract
{
	internal static List<Paragraph> Extract(FlowDocument doc, int start, int end)
	{
		var result = new List<Paragraph>();
		var col = TextTree.RootBlocks(doc, out int off);
		if (col != null) Blocks(col, start, end, ref off, result);
		return result;
	}

	static void Blocks(BlockCollection col, int s, int e, ref int off, List<Paragraph> result)
	{
		foreach (var b in col)
		{
			int a = off, size = TextTreeEdit.Size(b);
			off = a + size;
			if (a + size <= s || a >= e) continue;
			int inner = a + 1;
			if (b is Paragraph p)
			{
				var copy = new Paragraph();
				TextTreeEdit.CopyFormat(p, copy);
				Inlines(p.Inlines, s, e, ref inner, copy.Inlines);
				if (copy.Inlines.Count > 0 || (a >= s && a + size <= e)) result.Add(copy);
			}
			else if (b is Section sec) Blocks(sec.Blocks, s, e, ref inner, result);
			else if (b is List l)
				foreach (var item in l.ListItems)
				{
					int itemOff = inner + 1;
					Blocks(item.Blocks, s, e, ref itemOff, result);
					inner += TextTreeEdit.Size(item);
				}
		}
	}

	static void Inlines(InlineCollection col, int s, int e, ref int off, InlineCollection into)
	{
		foreach (var i in col)
		{
			int a = off, size = TextTreeEdit.Size(i);
			off = a + size;
			if (a + size <= s || a >= e) continue;
			if (i is Run r)
			{
				int from = Math.Max(s, a + 1) - (a + 1);
				int to = Math.Min(e, a + 1 + r.Text.Length) - (a + 1);
				if (to <= from) continue;
				var copy = new Run { Text = r.Text.Substring(from, to - from) };
				TextTreeEdit.CopyFormat(r, copy);
				into.Add(copy);
			}
			else if (i is Span sp)
			{
				var shell = TextTreeEdit.CloneShell(sp);
				int inner = a + 1;
				Inlines(sp.Inlines, s, e, ref inner, shell.Inlines);
				if (shell.Inlines.Count > 0) into.Add(shell);
			}
			else if (i is LineBreak && a >= s) into.Add(new LineBreak());
		}
	}
}
