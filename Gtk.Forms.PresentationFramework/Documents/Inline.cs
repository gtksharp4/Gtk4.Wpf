using System;
using System.Windows.Media;

namespace System.Windows.Documents
{
	/// <summary>Base class for all inline flow-content elements.</summary>
	public abstract class Inline : TextElement
	{
		/// <summary>
		/// Builds the Pango markup fragment for this inline and appends it to
		/// <paramref name="sb"/>.
		/// </summary>
		internal abstract void AppendMarkup(System.Text.StringBuilder sb);
	}

	/// <summary>Contains a run of uniformly-formatted text.</summary>
	public class Run : Inline
	{
		/// <summary>The text content of this run.</summary>
		public string Text { get; set; } = string.Empty;

		/// <inheritdoc/>
		internal override void AppendMarkup(System.Text.StringBuilder sb)
		{
			bool hasFg = Foreground is SolidColorBrush scb;
			bool hasBold = FontWeight.HasValue && FontWeight.Value == FontWeights.Bold;
			bool hasItalic = FontStyle.HasValue && FontStyle.Value == FontStyles.Italic;

			if (hasFg)
			{
				var color = ((SolidColorBrush)Foreground!).Color;
				sb.Append($"<span foreground=\"#{color.R:X2}{color.G:X2}{color.B:X2}\">");
			}
			if (hasBold) sb.Append("<b>");
			if (hasItalic) sb.Append("<i>");

			sb.Append(PangoMarkup.Escape(Text));

			if (hasItalic) sb.Append("</i>");
			if (hasBold) sb.Append("</b>");
			if (hasFg) sb.Append("</span>");
		}
	}

	/// <summary>Groups inline elements and applies shared formatting.</summary>
	public class Span : Inline
	{
		/// <summary>Child inlines in this span.</summary>
		public InlineCollection Inlines { get; } = new InlineCollection();

		/// <inheritdoc/>
		internal override void AppendMarkup(System.Text.StringBuilder sb)
		{
			foreach (var child in Inlines)
				child.AppendMarkup(sb);
		}
	}

	/// <summary>Renders content in bold.</summary>
	public class Bold : Span
	{
		/// <inheritdoc/>
		internal override void AppendMarkup(System.Text.StringBuilder sb)
		{
			sb.Append("<b>");
			base.AppendMarkup(sb);
			sb.Append("</b>");
		}
	}

	/// <summary>Renders content in italic.</summary>
	public class Italic : Span
	{
		/// <inheritdoc/>
		internal override void AppendMarkup(System.Text.StringBuilder sb)
		{
			sb.Append("<i>");
			base.AppendMarkup(sb);
			sb.Append("</i>");
		}
	}

	/// <summary>Renders content with an underline.</summary>
	public class Underline : Span
	{
		/// <inheritdoc/>
		internal override void AppendMarkup(System.Text.StringBuilder sb)
		{
			sb.Append("<u>");
			base.AppendMarkup(sb);
			sb.Append("</u>");
		}
	}

	/// <summary>Inserts a line break in an inline context.</summary>
	public class LineBreak : Inline
	{
		/// <inheritdoc/>
		internal override void AppendMarkup(System.Text.StringBuilder sb) =>
			sb.Append('\n');
	}

	// ── Shared helper ────────────────────────────────────────────────────────────

	/// <summary>Pango markup escaping (accessible to TextBlock for internal testing).</summary>
	internal static class PangoMarkup
	{
		/// <summary>Escapes characters that have special meaning in Pango markup XML.</summary>
		internal static string Escape(string? text)
		{
			if (string.IsNullOrEmpty(text)) return string.Empty;
			return text
				.Replace("&", "&amp;")
				.Replace("<", "&lt;")
				.Replace(">", "&gt;")
				.Replace("\"", "&quot;");
		}
	}
}
