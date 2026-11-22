using System.Linq;

namespace System.Windows.Documents;

static partial class TextTreeEdit
{
	/// <summary>
	/// Inserts <paramref name="inline"/> at <paramref name="offset"/> in the Paragraph or Span there; inside a Run's
	/// text the Run is split around it, as WPF does.
	/// </summary>
	/// <exception cref="InvalidOperationException">The position is not inside a Paragraph, Span or Run.</exception>
	internal static void InsertInline(FlowDocument doc, int offset, Inline inline)
	{
		var syms = TextTree.Build(doc);
		int before = TextTree.Total(syms);
		var open = TextTree.OpenElements(syms, offset);
		var top = open.Count == 0 ? null : open[open.Count - 1];
		if (top is Run run && run.Parent is Paragraph or Span)
			InsertInsideRun(syms, run, offset, inline);
		else
		{
			var col = top is Paragraph p ? p.Inlines : top is Span s ? s.Inlines : null;
			if (col == null) throw new InvalidOperationException("The position cannot hold an inline.");
			var items = col.ToList();
			items.Insert(items.Count(i => StartOffset(syms, i) < offset), inline);
			Replace(col, items);
		}
		doc.RecordTextEdit(offset, 0, TextTree.Total(TextTree.Build(doc)) - before);
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
