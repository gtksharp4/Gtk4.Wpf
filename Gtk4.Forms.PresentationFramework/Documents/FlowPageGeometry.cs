using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// Page, padding and column geometry of a <see cref="FlowDocument"/>, resolved as WPF's PtsHelper does. Pure
/// arithmetic, no GTK. Sizes are device-independent pixels.
/// </summary>
sealed class FlowPageGeometry
{
	FlowPageGeometry(Size page, Thickness padding, double lineHeight, double columnWidth, double gap, int columns)
	{
		Page = page;
		Padding = padding;
		LineHeight = lineHeight;
		ColumnWidth = columnWidth;
		ColumnGap = gap;
		Columns = columns;
	}

	internal Size Page { get; }

	/// <summary>PagePadding with auto (NaN) sides resolved to one line height.</summary>
	internal Thickness Padding { get; }

	/// <summary>LineHeight, or FontFamily.LineSpacing times FontSize when that is NaN.</summary>
	internal double LineHeight { get; }

	internal double ColumnWidth { get; }

	internal double ColumnGap { get; }

	internal int Columns { get; }

	internal double ContentWidth => Math.Max(1, Page.Width - Padding.Left - Padding.Right);

	internal double ContentHeight => Math.Max(1, Page.Height - Padding.Top - Padding.Bottom);

	/// <summary>Left edge of column <paramref name="index"/> in page coordinates.</summary>
	internal double ColumnX(int index) => Padding.Left + index * (ColumnWidth + ColumnGap);

	internal static double LineHeightOf(FlowDocument doc)
	{
		double explicitHeight = doc.LineHeight;
		if (!double.IsNaN(explicitHeight) && explicitHeight > 0) return explicitHeight;
		double spacing = ((FontFamily)doc.GetValue(FlowDocument.FontFamilyProperty)!).LineSpacing;
		double size = (double)doc.GetValue(FlowDocument.FontSizeProperty)!;
		return (spacing > 0 && !double.IsNaN(spacing) ? spacing : 1.2) * size;
	}

	/// <summary>
	/// The page size: PageWidth/PageHeight, or <paramref name="fallback"/> (the viewer's content area) when NaN,
	/// then clamped by Min/Max. Min wins over Max, as in WPF.
	/// </summary>
	internal static Size PageSize(FlowDocument doc, Size fallback)
	{
		double w = double.IsNaN(doc.PageWidth) ? fallback.Width : doc.PageWidth;
		double h = double.IsNaN(doc.PageHeight) ? fallback.Height : doc.PageHeight;
		w = Math.Max(doc.MinPageWidth, Math.Min(w, doc.MaxPageWidth));
		h = Math.Max(doc.MinPageHeight, Math.Min(h, doc.MaxPageHeight));
		return new Size(w, h);
	}

	internal static FlowPageGeometry Resolve(FlowDocument doc, Size page)
	{
		double lineHeight = LineHeightOf(doc);
		var raw = doc.PagePadding;
		var padding = new Thickness(Auto(raw.Left, lineHeight), Auto(raw.Top, lineHeight),
			Auto(raw.Right, lineHeight), Auto(raw.Bottom, lineHeight));
		double contentWidth = Math.Max(1, page.Width - padding.Left - padding.Right);
		double fontSize = (double)doc.GetValue(FlowDocument.FontSizeProperty)!;
		double gap = double.IsNaN(doc.ColumnGap) ? lineHeight : doc.ColumnGap;
		double width = double.IsNaN(doc.ColumnWidth) ? 20 * fontSize : doc.ColumnWidth;
		int columns = Math.Max(1, (int)((contentWidth + gap) / (width + gap)));
		width = Math.Max(1, Math.Min(width, contentWidth));
		double free = Math.Max(0, contentWidth - columns * width - (columns - 1) * gap);
		// Non-flexible: the gap stays and the unused width stays on the right (PtsHelper.GetColumnsInfo).
		if (doc.IsColumnWidthFlexible) width += free / columns;
		return new FlowPageGeometry(page, padding, lineHeight, width, gap, columns);
	}

	static double Auto(double side, double lineHeight) => double.IsNaN(side) ? lineHeight : side;
}
