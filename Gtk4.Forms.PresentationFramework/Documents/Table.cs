using System.Text;
using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>
/// A grid of rows and cells. Rendered as text: one line per row, cells separated by a tab, with the viewer's
/// label tab stops set from the cell text lengths (an approximation: a label has one tab array and no metrics
/// are available). Gap: column widths, spans, cell borders, padding and column/row backgrounds are stored but
/// not drawn.
/// </summary>
[ContentProperty("RowGroups")]
public partial class Table : Block
{
	/// <summary>Identifies the CellSpacing property (default 2; finite and not negative).</summary>
	public static readonly DependencyProperty CellSpacingProperty = DependencyProperty.Register(
		nameof(CellSpacing), typeof(double), typeof(Table), new FrameworkPropertyMetadata(2.0),
		v => v is double d && !double.IsNaN(d) && !double.IsInfinity(d) && d >= 0);

	/// <summary>Creates an empty table.</summary>
	public Table()
	{
		Columns = new TableColumnCollection(this);
		RowGroups = new TableRowGroupCollection(this);
	}

	/// <summary>Gets or sets the space between cells.</summary>
	public double CellSpacing
	{
		get => (double)GetValue(CellSpacingProperty)!;
		set => SetValue(CellSpacingProperty, value);
	}

	/// <summary>The column definitions.</summary>
	public TableColumnCollection Columns { get; }

	/// <summary>The row groups; the XAML content property.</summary>
	public TableRowGroupCollection RowGroups { get; }

	/// <inheritdoc/>
	internal override void AppendPlainText(StringBuilder sb)
	{
		bool first = true;
		foreach (var row in Rows())
		{
			if (!first) sb.Append('\n');
			first = false;
			row.AppendPlainText(sb);
		}
	}

	/// <inheritdoc/>
	internal override void AppendMarkup(StringBuilder sb) => AppendRows(sb, null);

	/// <inheritdoc/>
	internal override void AppendFlowMarkup(StringBuilder sb, FlowDirection? effective) =>
		AppendLanguage(sb, s => AppendRows(s, effective));

	// Each row line is its own bidi paragraph, so it carries its own isolate (as List items do).
	void AppendRows(StringBuilder sb, FlowDirection? effective)
	{
		bool first = true;
		foreach (var group in RowGroups)
			group.AppendLanguage(sb, s =>
			{
				foreach (var row in group.Rows)
				{
					if (!first) s.Append('\n');
					first = false;
					if ((row.ExplicitFlowDirection ?? effective) is FlowDirection d)
						BidiMarks.AppendIsolated(s, d, true, t => row.AppendMarkup(t));
					else row.AppendMarkup(s);
				}
			});
	}

	System.Collections.Generic.IEnumerable<TableRow> Rows()
	{
		foreach (var group in RowGroups)
			foreach (var row in group.Rows)
				yield return row;
	}
}
