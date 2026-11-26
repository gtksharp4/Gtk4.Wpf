namespace System.Windows.Documents;

static partial class TextTree
{
	/// <summary>
	/// The number of plain-text characters before <paramref name="element"/> (text plus one per paragraph end)
	/// and in the whole document. An element the tree does not walk (table content) counts as its nearest
	/// ancestor that it does.
	/// </summary>
	internal static void PlainChars(FlowDocument doc, TextElement element, out int before, out int total)
	{
		var syms = Build(doc);
		int stop = -1;
		for (DependencyObject? e = element; e != null && stop < 0; e = (e as FrameworkContentElement)?.Parent)
			foreach (var sym in syms)
				if (sym.Kind == TextSymKind.Start && ReferenceEquals(sym.Node, e)) { stop = sym.Offset; break; }
		before = total = 0;
		foreach (var sym in syms)
		{
			int chars = sym.Kind == TextSymKind.Text ? sym.Length
				: sym.Kind == TextSymKind.End && sym.Node is Paragraph ? 1 : 0;
			total += chars;
			if (stop >= 0 && sym.Offset < stop) before += chars;
		}
	}
}
