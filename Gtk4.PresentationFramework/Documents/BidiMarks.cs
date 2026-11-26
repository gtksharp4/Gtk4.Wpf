using System.Text;

namespace System.Windows.Documents;

/// <summary>Unicode bidi controls used to render <see cref="FlowDirection"/> in Pango text.</summary>
internal static class BidiMarks
{
	internal const char Lri = '\u2066';
	internal const char Rli = '\u2067';
	internal const char Pdi = '\u2069';
	internal const char Lrm = '\u200E';
	internal const char Rlm = '\u200F';

	/// <summary>The isolate opener for <paramref name="direction"/>.</summary>
	internal static char Isolate(FlowDirection direction) =>
		direction == FlowDirection.RightToLeft ? Rli : Lri;

	/// <summary>The directional mark for <paramref name="direction"/>; it fixes a paragraph's base direction.</summary>
	internal static char Mark(FlowDirection direction) =>
		direction == FlowDirection.RightToLeft ? Rlm : Lrm;

	/// <summary>
	/// Appends the output of <paramref name="body"/> inside an isolate. Pango splits paragraphs at every
	/// <c>'\n'</c> and an isolate cannot span them, so each line closes and reopens it. A block-level isolate
	/// (<paramref name="markFirst"/>) starts every line with the directional mark; an inline one has none.
	/// </summary>
	internal static void AppendIsolated(StringBuilder sb, FlowDirection direction, bool markFirst,
		Action<StringBuilder> body)
	{
		var inner = new StringBuilder();
		body(inner);
		string text = inner.ToString();
		if (markFirst) sb.Append(Mark(direction));
		sb.Append(Isolate(direction));
		foreach (char c in text)
		{
			if (c != '\n') { sb.Append(c); continue; }
			sb.Append(Pdi).Append('\n');
			if (markFirst) sb.Append(Mark(direction));
			sb.Append(Isolate(direction));
		}
		sb.Append(Pdi);
	}

	/// <summary>Inserts <paramref name="mark"/> after every <c>'\n'</c> so each Pango paragraph gets the direction.</summary>
	internal static string MarkLines(string text, char mark) =>
		text.IndexOf('\n') < 0 ? text : text.Replace("\n", "\n" + mark);
}
