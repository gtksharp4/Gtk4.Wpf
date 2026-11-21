using System.Collections.Generic;

namespace System.Windows.Documents;

public partial class FlowDocument
{
	// Approximate column geometry for a Pango label: it has one tab array for the whole text and no text metrics
	// are available here, so a column is as wide as its longest cell at an average glyph width, plus padding.
	const int TabCharWidth = 8;
	const int TabPadding = 24;

	/// <summary>
	/// Pixel tab positions that line up the cells of the document's tables (the widest column across all tables
	/// wins), or <c>null</c> when there is no table with more than one column. Cell spans are not considered.
	/// </summary>
	internal int[]? BuildTabStops()
	{
		var widths = new List<int>();
		CollectColumnWidths(Blocks, widths);
		if (widths.Count < 2) return null;
		var stops = new int[widths.Count - 1];
		int position = 0;
		for (int i = 0; i < stops.Length; i++) stops[i] = position += widths[i];
		return stops;
	}

	static void CollectColumnWidths(IEnumerable<Block> blocks, List<int> widths)
	{
		foreach (var block in blocks)
		{
			if (block is Section section) CollectColumnWidths(section.Blocks, widths);
			if (!(block is Table table)) continue;
			foreach (var group in table.RowGroups)
				foreach (var row in group.Rows)
				{
					int column = 0;
					foreach (var cell in row.Cells)
					{
						var text = new System.Text.StringBuilder();
						cell.AppendPlainText(text);
						int width = text.Length * TabCharWidth + TabPadding;
						while (widths.Count <= column) widths.Add(0);
						if (width > widths[column]) widths[column] = width;
						column++;
					}
				}
		}
	}
}
