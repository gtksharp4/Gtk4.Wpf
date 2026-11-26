using System.Linq;

namespace System.Windows.Documents;

static partial class TextTreeEdit
{
	/// <summary>
	/// Inserts <paramref name="inline"/> at <paramref name="offset"/> in the Paragraph or Span there; inside a Run's
	/// text the Run is split around it. Between blocks, or in an empty document, Section or ListItem, a Paragraph
	/// is created at the position to hold it, as WPF's EnsureInsertionPosition does; any other position (between
	/// list items or in a table part) first moves to the next insertion position.
	/// </summary>
	/// <exception cref="InvalidOperationException">The position is inside a BlockUIContainer or InlineUIContainer,
	/// or no insertion position can be found.</exception>
	internal static void InsertInline(FlowDocument doc, int offset, Inline inline, bool moved = false)
	{
		var syms = doc.Symbols;
		int before = TextTree.Total(syms);
		var open = TextTree.OpenElements(syms, offset);
		var top = open.Count == 0 ? null : open[open.Count - 1];
		if (top is Run run && run.Parent is Paragraph or Span)
			InsertInsideRun(syms, run, offset, inline);
		else if (top is Paragraph p)
			InsertAmong(syms, p.Inlines, offset, inline);
		else if (top is Span s)
			InsertAmong(syms, s.Inlines, offset, inline);
		else if (top is InlineUIContainer || top is BlockUIContainer)
			throw new InvalidOperationException("The position is inside an embedded element.");
		else if (top == null && doc.DetachedRoot == null || top is Section || top is ListItem)
			InsertInNewParagraph(doc, syms, top ?? doc, offset, inline);
		else
		{
			int next = new TextPointer(doc, offset, LogicalDirection.Forward)
				.GetInsertionPosition(LogicalDirection.Forward).Offset;
			if (moved || next == offset) throw new InvalidOperationException("The position cannot hold an inline.");
			InsertInline(doc, next, inline, moved: true);
			return;
		}
		doc.RecordTextEdit(offset, 0, doc.SymbolTotal - before);
	}

	static void InsertAmong(System.Collections.Generic.List<TextSym> syms, InlineCollection col, int offset,
		Inline inline)
	{
		var items = col.ToList();
		items.Insert(items.Count(i => StartOffset(syms, i) < offset), inline);
		Replace(col, items);
	}

	static void InsertInNewParagraph(FlowDocument doc, System.Collections.Generic.List<TextSym> syms, object parent,
		int offset, Inline inline)
	{
		var col = parent is FlowDocument ? doc.Blocks : parent is Section sec ? sec.Blocks : ((ListItem)parent).Blocks;
		var items = col.ToList();
		var paragraph = new Paragraph();
		paragraph.Inlines.Add(inline);
		items.Insert(items.Count(b => StartOffset(syms, b) < offset), paragraph);
		Replace(col, items);
	}

	static void InsertInsideRun(System.Collections.Generic.List<TextSym> syms, Run run, int offset, Inline inline)
	{
		var col = run.Parent is Paragraph p ? p.Inlines : ((Span)run.Parent!).Inlines;
		int local = offset - (StartOffset(syms, run) + 1);
		var items = col.ToList();
		int at = items.IndexOf(run);
		if (local >= run.Text.Length) items.Insert(at + 1, inline);
		else if (local <= 0) items.Insert(at, inline);
		else
		{
			var right = new Run { Text = run.Text.Substring(local) };
			var values = run.GetLocalValueEnumerator();
			while (values.MoveNext())
			{
				var e = values.Current;
				if (e.Property != Run.TextProperty && !e.Property.IsReadOnly) right.SetValue(e.Property, e.Value);
			}
			run.Text = run.Text.Substring(0, local);
			items.Insert(at + 1, inline);
			items.Insert(at + 2, right);
		}
		Replace(col, items);
	}
}
