using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>A group of rows in a <see cref="Table"/>.</summary>
[ContentProperty("Rows")]
public class TableRowGroup : TextElement
{
	/// <summary>Creates an empty row group.</summary>
	public TableRowGroup() => Rows = new TableRowCollection(this);

	/// <summary>The rows; the XAML content property.</summary>
	public TableRowCollection Rows { get; }
}

/// <summary>A row of <see cref="TableCell"/>s.</summary>
[ContentProperty("Cells")]
public class TableRow : TextElement
{
	/// <summary>Creates an empty row.</summary>
	public TableRow() => Cells = new TableCellCollection(this);

	/// <summary>The cells; the XAML content property.</summary>
	public TableCellCollection Cells { get; }

	internal void AppendPlainText(System.Text.StringBuilder sb)
	{
		bool first = true;
		foreach (var cell in Cells)
		{
			if (!first) sb.Append('\t');
			first = false;
			cell.AppendPlainText(sb);
		}
	}

	internal void AppendMarkup(System.Text.StringBuilder sb) => AppendLanguage(sb, s =>
	{
		bool first = true;
		foreach (var cell in Cells)
		{
			if (!first) s.Append('\t');
			first = false;
			cell.AppendMarkup(s);
		}
	});
}
