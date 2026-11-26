using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// Wraps a <see cref="FlowBox"/> to its text width with Pango, or with an average-glyph estimate when GTK has no
/// display. <see cref="Configure"/> is shared with the page view so what is measured is what is drawn.
/// </summary>
static class FlowBoxText
{
	const double Scale = 1024.0;

	internal static void Measure(FlowBox box, FlowDocument doc)
	{
		var map = GtkFontCatalog.FontMap;
		if (map != null)
		{
			try
			{
				MeasurePango(box, doc, map);
				EmptyParagraphIsOneLine(box);
				return;
			}
			catch (Exception) { /* fall through to the estimate */ }
		}
		MeasureEstimate(box, (double)doc.GetValue(FlowDocument.FontSizeProperty)!);
		EmptyParagraphIsOneLine(box);
	}

	// PTS formats an empty Paragraph as one blank line (its paragraph-end line); it never collapses through.
	static void EmptyParagraphIsOneLine(FlowBox box)
	{
		if (!box.IsParagraph || box.Lines.Count > 0) return;
		box.Lines = new List<string> { string.Empty };
		box.LineStarts = new List<int> { 0 };
	}

	/// <summary>Font, width, alignment, indent and extra line spacing of <paramref name="box"/> on a layout.</summary>
	internal static void Configure(Pango.Layout layout, FlowBox box, FlowDocument doc, double pitch)
	{
		using (var desc = new Pango.FontDescription())
		{
			desc.Family = ((FontFamily)doc.GetValue(FlowDocument.FontFamilyProperty)!).Source;
			desc.AbsoluteSize = (double)doc.GetValue(FlowDocument.FontSizeProperty)! * Scale;
			desc.Weight = (Pango.Weight)((FontWeight)doc.GetValue(FlowDocument.FontWeightProperty)!).ToOpenTypeWeight();
			var style = (FontStyle)doc.GetValue(FlowDocument.FontStyleProperty)!;
			desc.Style = style == FontStyles.Italic ? Pango.Style.Italic
				: style == FontStyles.Oblique ? Pango.Style.Oblique : Pango.Style.Normal;
			layout.FontDescription = desc;
		}
		layout.Width = (int)(box.TextWidth * Scale);
		layout.Wrap = Pango.WrapMode.WordChar;
		layout.Alignment = box.Alignment == TextAlignment.Center ? Pango.Alignment.Center
			: box.Alignment == TextAlignment.Right ? Pango.Alignment.Right : Pango.Alignment.Left;
		layout.Justify = box.Alignment == TextAlignment.Justify;
		layout.Indent = (int)(box.TextIndent * Scale);
		layout.Spacing = (int)(Math.Max(0, pitch - NaturalPitch(layout)) * Scale);
	}

	static double NaturalPitch(Pango.Layout layout)
	{
		layout.Spacing = 0;
		layout.GetSize(out _, out int height);
		return layout.LineCount == 0 ? 0 : height / Scale / layout.LineCount;
	}

	static void MeasurePango(FlowBox box, FlowDocument doc, Pango.FontMap map)
	{
		var layout = new Pango.Layout(map.CreateContext());
		layout.SetMarkup(box.Markup);
		Configure(layout, box, doc, 0);
		layout.Spacing = 0;
		layout.GetSize(out _, out int height);
		int count = layout.LineCount;
		double natural = count == 0 ? 0 : height / Scale / count;
		box.Pitch = !double.IsNaN(box.LineHeight) && box.LineHeight > 0 ? box.LineHeight : natural;
		var bytes = Encoding.UTF8.GetBytes(layout.Text);
		var lines = new List<string>();
		var starts = new List<int>();
		for (int i = 0; i < count; i++)
		{
			var line = layout.GetLineReadonly(i);
			starts.Add(Encoding.UTF8.GetString(bytes, 0, line.StartIndex).Length);
			lines.Add(Encoding.UTF8.GetString(bytes, line.StartIndex, line.Length).TrimEnd('\n'));
		}
		if (bytes.Length == 0) { lines.Clear(); starts.Clear(); }
		box.Lines = lines;
		box.LineStarts = starts;
	}

	static void MeasureEstimate(FlowBox box, double size)
	{
		box.Pitch = !double.IsNaN(box.LineHeight) && box.LineHeight > 0 ? box.LineHeight : size * 1.2;
		int perLine = Math.Max(1, (int)(box.TextWidth / (size * 0.5)));
		var lines = new List<string>();
		var starts = new List<int>();
		int offset = 0;
		if (box.Plain.Length > 0)
		{
			foreach (var para in box.Plain.Split('\n'))
			{
				for (int i = 0; i < para.Length || (i == 0 && para.Length == 0); i += perLine)
				{
					starts.Add(offset + i);
					lines.Add(para.Substring(i, Math.Min(perLine, para.Length - i)));
					if (para.Length == 0) break;
				}
				offset += para.Length + 1;
			}
		}
		box.Lines = lines;
		box.LineStarts = starts;
	}
}
