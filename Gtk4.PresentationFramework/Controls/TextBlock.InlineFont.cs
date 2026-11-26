using System.Windows.Documents;

namespace System.Windows.Controls;

public partial class TextBlock
{
	// An inline's own FontFamily (set by a TextRange, for one) is rendered as a Pango font_family span.
	static string? LocalFamily(Inline inline) => TextLocalValues.Family(inline)?.Source;

	static void AppendInline(System.Text.StringBuilder sb, Inline inline)
	{
		string? family = LocalFamily(inline);
		if (family == null) { inline.AppendWrappedMarkup(sb); return; }
		sb.Append("<span font_family=\"").Append(Documents.PangoMarkup.Escape(family)).Append("\">");
		inline.AppendWrappedMarkup(sb);
		sb.Append("</span>");
	}
}
