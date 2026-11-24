using System.Collections.Generic;

namespace System.Windows.Documents;

/// <summary>
/// Lays a <see cref="FlowDocument"/> out into pages and columns, block by block. Each top-level block is its own
/// Pango layout (inline markup, TextAlignment, LineHeight, TextIndent) wrapped to the column width; blocks stack
/// with WPF's collapsing vertical margins and their lines flow column by column and page by page. Without a GTK
/// display the wrap uses an average glyph width, which keeps the count deterministic.
/// <para>
/// Differences from WPF's PTS: a paragraph splits between columns or pages at any line (no widows/orphans,
/// KeepTogether or KeepWithNext), a top margin is dropped at the head of a column, an empty block collapses its
/// margins through, and tables, lists and floaters are laid out as one block of text.
/// </para>
/// </summary>
sealed class FlowDocumentPageMeter
{
	internal const double DefaultWidth = 816;
	internal const double DefaultHeight = 1056;

	FlowDocumentPageMeter(FlowPageGeometry geometry, List<FlowBox> boxes, List<FlowPage> pages)
	{
		Geometry = geometry;
		Boxes = boxes;
		Pages = pages;
	}

	internal FlowPageGeometry Geometry { get; }

	internal List<FlowBox> Boxes { get; }

	internal List<FlowPage> Pages { get; }

	internal int PageCount => Pages.Count;

	/// <summary>The zero-based page holding the line that contains character <paramref name="chars"/>.</summary>
	internal int PageOfChar(int chars)
	{
		FlowBox? box = null;
		foreach (var b in Boxes)
		{
			if (b.CharStart > chars) break;
			box = b;
		}
		if (box == null) return 0;
		int inBox = chars - box.CharStart, line = 0;
		for (int i = 0; i < box.LineStarts.Count && box.LineStarts[i] <= inBox; i++) line = i;
		for (int p = 0; p < Pages.Count; p++)
			foreach (var column in Pages[p].Columns)
				foreach (var slice in column.Slices)
					if (ReferenceEquals(slice.Box, box) && line >= slice.First && line < slice.First + slice.Count)
						return p;
		return 0;
	}

	internal static FlowDocumentPageMeter Measure(FlowDocument doc, Size page)
	{
		var geometry = FlowPageGeometry.Resolve(doc, page);
		var boxes = FlowBox.Build(doc, geometry.LineHeight);
		foreach (var box in boxes)
		{
			box.TextWidth = Math.Max(1, geometry.ColumnWidth - box.Margin.Left - box.Margin.Right
				- box.Padding.Left - box.Padding.Right);
			FlowBoxText.Measure(box, doc);
		}
		return new FlowDocumentPageMeter(geometry, boxes, Flow(geometry, boxes));
	}

	static List<FlowPage> Flow(FlowPageGeometry g, List<FlowBox> boxes)
	{
		var pages = new List<FlowPage> { new FlowPage(g.Columns) };
		int column = 0;
		double y = 0, pending = 0, height = g.ContentHeight;
		bool columnEmpty = true;
		void Advance()
		{
			if (++column >= g.Columns)
			{
				column = 0;
				pages.Add(new FlowPage(g.Columns));
			}
			y = 0;
			columnEmpty = true;
		}
		foreach (var box in boxes)
		{
			pending = Math.Max(pending, box.Margin.Top);
			if (box.Lines.Count == 0)
			{
				pending = Math.Max(pending, box.Margin.Bottom);
				continue;
			}
			double lead = columnEmpty ? 0 : pending;
			double pad = box.Padding.Top;
			if (!columnEmpty && y + lead + pad + box.Pitch > height) Advance();
			else y += lead;
			y += pad;
			pending = 0;
			FlowSlice? slice = null;
			for (int line = 0; line < box.Lines.Count; line++)
			{
				if (!columnEmpty && y + box.Pitch > height + 0.01) Advance();
				var col = pages[pages.Count - 1].Columns[column];
				if (slice == null || col.Slices.Count == 0 || !ReferenceEquals(col.Slices[col.Slices.Count - 1], slice))
				{
					slice = new FlowSlice(box, line, 0, y);
					col.Slices.Add(slice);
				}
				slice.Count++;
				y += box.Pitch;
				columnEmpty = false;
			}
			y += box.Padding.Bottom;
			pending = box.Margin.Bottom;
		}
		return TrimEmptyTail(pages);
	}

	// A margin that spilled onto a fresh page must not add an empty page.
	static List<FlowPage> TrimEmptyTail(List<FlowPage> pages)
	{
		while (pages.Count > 1 && pages[pages.Count - 1].Columns.TrueForAll(c => c.Slices.Count == 0))
			pages.RemoveAt(pages.Count - 1);
		return pages;
	}
}
