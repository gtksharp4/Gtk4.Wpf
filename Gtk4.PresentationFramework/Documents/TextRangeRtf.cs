using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// A documented RTF subset: <c>{\rtf1</c>, <c>\par</c>, <c>\line</c>, <c>\b</c>, <c>\i</c>, <c>\ul</c>, <c>\fsN</c>,
/// <c>\'hh</c> and <c>\uN</c> escapes, with groups scoping the character formatting. Colours, fonts and
/// everything else are skipped on read and not written.
/// </summary>
static class TextRangeRtf
{
	internal static string Write(List<Paragraph> paragraphs)
	{
		var sb = new StringBuilder("{\\rtf1\\ansi\\deff0\r\n");
		foreach (var p in paragraphs)
		{
			sb.Append("\\pard ");
			Inlines(sb, p.Inlines, false, false, false);
			sb.Append("\\par\r\n");
		}
		return sb.Append('}').ToString();
	}

	static void Inlines(StringBuilder sb, InlineCollection col, bool bold, bool italic, bool underline)
	{
		foreach (var i in col)
		{
			bool b = bold || i is Bold || (TextLocalValues.Weight(i)?.ToOpenTypeWeight() ?? 400) >= 700;
			bool it = italic || i is Italic || (TextLocalValues.Style(i) is FontStyle fs && fs != FontStyles.Normal);
			bool ul = underline || i is Underline || IsUnderlined(i);
			if (i is LineBreak) sb.Append("\\line ");
			else if (i is Span s) Inlines(sb, s.Inlines, b, it, ul);
			else if (i is Run r)
			{
				sb.Append('{');
				if (b) sb.Append("\\b");
				if (it) sb.Append("\\i");
				if (ul) sb.Append("\\ul");
				var size = TextLocalValues.Size(r);
				if (size != null) sb.Append("\\fs").Append((int)Math.Round(size.Value * 1.5));
				if (sb[sb.Length - 1] != '{') sb.Append(' ');
				Escape(sb, r.Text);
				sb.Append('}');
			}
		}
	}

	/// <summary>Whether the element carries an Underline-location text decoration.</summary>
	internal static bool IsUnderlined(TextElement e)
	{
		if (e.TextDecorations == null) return false;
		foreach (var d in e.TextDecorations)
			if (d.Location == TextDecorationLocation.Underline) return true;
		return false;
	}

	static void Escape(StringBuilder sb, string text)
	{
		foreach (char c in text)
		{
			if (c == '\\' || c == '{' || c == '}') sb.Append('\\').Append(c);
			else if (c > 126) sb.Append("\\u").Append((short)c).Append('?');
			else sb.Append(c);
		}
	}
}
