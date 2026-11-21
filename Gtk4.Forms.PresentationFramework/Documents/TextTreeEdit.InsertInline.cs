using System.Linq;

namespace System.Windows.Documents;

static partial class TextTreeEdit
{
	/// <summary>Inserts <paramref name="inline"/> at <paramref name="offset"/> in the Paragraph or Span there.</summary>
	/// <exception cref="InvalidOperationException">The position is not inside a Paragraph or Span.</exception>
	internal static void InsertInline(FlowDocument doc, int offset, Inline inline)
	{
		var syms = TextTree.Build(doc);
		int before = TextTree.Total(syms);
		var open = TextTree.OpenElements(syms, offset);
		var col = open.Count == 0 ? null
			: open[open.Count - 1] is Paragraph p ? p.Inlines
			: open[open.Count - 1] is Span s ? s.Inlines : null;
		if (col == null) throw new InvalidOperationException("The position cannot hold an inline.");
		var items = col.ToList();
		items.Insert(items.Count(i => StartOffset(syms, i) < offset), inline);
		Replace(col, items);
		doc.RecordTextEdit(offset, 0, TextTree.Total(TextTree.Build(doc)) - before);
	}
}
