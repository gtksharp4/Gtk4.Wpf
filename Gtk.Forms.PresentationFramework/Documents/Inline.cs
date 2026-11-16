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
			bool hasFg = Foreground is SolidColorBrush;
			bool hasBold = FontWeight.HasValue && FontWeight.Value == FontWeights.Bold;
			bool hasItalic = FontStyle.HasValue && FontStyle.Value == FontStyles.Italic;
			string? weightSpan = GetPangoWeightSpan(FontWeight);

			if (hasFg)
			{
				var color = ((SolidColorBrush)Foreground!).Color;
				sb.Append($"<span foreground=\"#{color.R:X2}{color.G:X2}{color.B:X2}\">");
			}
			if (weightSpan != null) sb.Append(weightSpan);
			if (hasBold) sb.Append("<b>");
			if (hasItalic) sb.Append("<i>");

			sb.Append(PangoMarkup.Escape(Text));

			if (hasItalic) sb.Append("</i>");
			if (hasBold) sb.Append("</b>");
			if (weightSpan != null) sb.Append("</span>");
			if (hasFg) sb.Append("</span>");
		}

		/// <summary>
		/// Returns a Pango <c>&lt;span weight='…'&gt;</c> open tag for non-Normal, non-Bold weights,
		/// or <c>null</c> when no weight span is needed (Normal/Bold/unset).
		/// Bold (700) is handled by the <c>&lt;b&gt;</c> path; Normal (400) emits nothing.
		/// </summary>
		static string? GetPangoWeightSpan(FontWeight? fontWeight)
		{
			if (!fontWeight.HasValue) return null;
			int w = fontWeight.Value.Weight;
			// Bold handled by <b> tag; Normal needs no markup.
			if (w == 700 || w == 400) return null;
			// Map to Pango named weights where available; fall back to numeric.
			string pangoWeight = w switch
			{
				100 => "ultralight",
				200 => "ultralight",
				300 => "light",
				500 => "medium",
				600 => "semibold",
				800 => "ultrabold",
				900 => "heavy",
				_ => w.ToString()
			};
			return $"<span weight='{pangoWeight}'>";
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
