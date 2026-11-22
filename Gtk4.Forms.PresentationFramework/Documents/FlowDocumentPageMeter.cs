using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// Splits a <see cref="FlowDocument"/>'s plain text into page lines. Approximation: the text is wrapped by one Pango
/// layout in the document font at the column width (page width minus PagePadding, split into ColumnWidth columns),
/// then cut every whole number of lines that fit the page height. Blocks, tables and embedded elements add no
/// extra spacing, so counts differ from WPF's; without a GTK display the wrap uses an average glyph width.
/// </summary>
sealed class FlowDocumentPageMeter
{
	internal const double DefaultWidth = 816;
	internal const double DefaultHeight = 1056;
	const double PaddingFloor = 1;

	internal FlowDocumentPageMeter(List<string> lines, double lineHeight, int columns, double columnWidth,
		double columnGap, int linesPerColumn, Thickness padding)
	{
		Lines = lines;
		LineHeight = lineHeight;
		Columns = columns;
		ColumnWidth = columnWidth;
		ColumnGap = columnGap;
		LinesPerColumn = linesPerColumn;
		Padding = padding;
	}

	internal List<string> Lines { get; }

	internal double LineHeight { get; }

	internal int Columns { get; }

	internal double ColumnWidth { get; }

	internal double ColumnGap { get; }

	internal int LinesPerColumn { get; }

	internal Thickness Padding { get; }

	internal int LinesPerPage => Columns * LinesPerColumn;

	internal int PageCount => Math.Max(1, (Lines.Count + LinesPerPage - 1) / LinesPerPage);

	internal static FlowDocumentPageMeter Measure(FlowDocument doc, Size page)
	{
		var padding = doc.PagePadding;
		double width = Math.Max(PaddingFloor, page.Width - padding.Left - padding.Right);
		double height = Math.Max(PaddingFloor, page.Height - padding.Top - padding.Bottom);
		double size = (double)doc.GetValue(FlowDocument.FontSizeProperty)!;
		string family = ((FontFamily)doc.GetValue(FlowDocument.FontFamilyProperty)!).Source;
		SplitColumns(doc, width, out int columns, out double colWidth, out double gap);
		var lines = Wrap(doc.BuildPlainText(), family, size, colWidth, out double lineHeight);
		int perColumn = Math.Max(1, (int)(height / lineHeight));
		return new FlowDocumentPageMeter(lines, lineHeight, columns, colWidth, gap, perColumn, padding);
	}

	static void SplitColumns(FlowDocument doc, double width, out int columns, out double colWidth, out double gap)
	{
		double cw = doc.ColumnWidth;
		gap = double.IsNaN(doc.ColumnGap) ? (double.IsNaN(cw) ? 0 : cw * 0.02) : doc.ColumnGap;
		columns = 1;
		colWidth = width;
		if (double.IsNaN(cw) || cw <= 0) return;
		columns = Math.Max(1, (int)((width + gap) / (cw + gap)));
		colWidth = (width - (columns - 1) * gap) / columns;
	}

	static List<string> Wrap(string text, string family, double size, double width, out double lineHeight)
	{
		var map = GtkFontCatalog.FontMap;
		if (map != null)
		{
			try { return WrapPango(map, text, family, size, width, out lineHeight); }
			catch (Exception) { /* fall through to the estimate */ }
		}
		return WrapEstimate(text, size, width, out lineHeight);
	}

	static List<string> WrapPango(Pango.FontMap map, string text, string family, double size, double width,
		out double lineHeight)
	{
		const double Scale = 1024.0;
		using (var desc = new Pango.FontDescription())
		{
			desc.Family = family;
			desc.AbsoluteSize = size * Scale;
			var ctx = map.CreateContext();
			ctx.FontDescription = desc;
			var layout = new Pango.Layout(ctx);
			layout.Width = (int)(width * Scale);
			layout.SetText(text);
			var bytes = Encoding.UTF8.GetBytes(text);
			var lines = new List<string>();
			for (int i = 0; i < layout.LineCount; i++)
			{
				var line = layout.GetLineReadonly(i);
				lines.Add(Encoding.UTF8.GetString(bytes, line.StartIndex, line.Length).TrimEnd('\n'));
			}
			layout.GetSize(out _, out int total);
			lineHeight = lines.Count == 0 ? size * 1.2 : total / Scale / lines.Count;
			return lines;
		}
	}

	static List<string> WrapEstimate(string text, double size, double width, out double lineHeight)
	{
		lineHeight = size * 1.2;
		int perLine = Math.Max(1, (int)(width / (size * 0.5)));
		var lines = new List<string>();
		foreach (var para in text.Split('\n'))
		{
			for (int i = 0; i < para.Length; i += perLine)
				lines.Add(para.Substring(i, Math.Min(perLine, para.Length - i)));
			if (para.Length == 0) lines.Add(string.Empty);
		}
		return lines;
	}
}
