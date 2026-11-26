using System.Collections.Generic;
using System.Text;

namespace System.Windows.Documents;

/// <summary>
/// One top-level block as the page meter lays it out: its own Pango markup wrapped to the column, its resolved
/// margins and the line texts. Only NaN margin sides are auto (WPF MbpInfo.IsMarginAuto): lineHeight above and
/// below for a Paragraph, List or Table, zero for other blocks; an explicit zero is honoured.
/// </summary>
sealed class FlowBox
{
	internal string Markup { get; private set; } = string.Empty;

	internal string Plain { get; private set; } = string.Empty;

	internal bool IsParagraph { get; private set; }

	/// <summary>Index of this block's first character in the text tree's plain count (one newline per block).</summary>
	internal int CharStart { get; private set; }

	internal Thickness Margin { get; private set; }

	internal Thickness Padding { get; private set; }

	internal TextAlignment Alignment { get; private set; }

	internal double TextIndent { get; private set; }

	/// <summary>The block's own LineHeight, or NaN to use the font's natural line pitch.</summary>
	internal double LineHeight { get; private set; } = double.NaN;

	/// <summary>Width the text wraps to: the column minus margins and padding.</summary>
	internal double TextWidth { get; set; }

	/// <summary>Distance between line tops.</summary>
	internal double Pitch { get; set; }

	internal List<string> Lines { get; set; } = new List<string>();

	internal List<int> LineStarts { get; set; } = new List<int>();

	internal static List<FlowBox> Build(FlowDocument doc, double lineHeight)
	{
		var boxes = new List<FlowBox>();
		int start = 0;
		foreach (var block in doc.Blocks)
		{
			var markup = new StringBuilder();
			block.AppendBlockMarkup(markup, doc.FlowDirectionForLayout);
			var plain = new StringBuilder();
			block.AppendPlainText(plain);
			boxes.Add(new FlowBox
			{
				Markup = markup.ToString(),
				Plain = plain.ToString(),
				IsParagraph = block is Paragraph,
				CharStart = start,
				Margin = ResolveMargin(block, lineHeight),
				Padding = Clean(block.Padding),
				Alignment = block.TextAlignment ?? doc.TextAlignment ?? TextAlignment.Left,
				TextIndent = (block as Paragraph)?.TextIndent ?? 0,
				LineHeight = double.IsNaN(block.LineHeight) ? doc.LineHeight : block.LineHeight,
			});
			start += plain.Length + 1;
		}
		return boxes;
	}

	static Thickness Clean(Thickness t) => new Thickness(Zero(t.Left), Zero(t.Top), Zero(t.Right), Zero(t.Bottom));

	static double Zero(double v) => double.IsNaN(v) ? 0 : v;

	static Thickness ResolveMargin(Block block, double lineHeight)
	{
		var m = block.Margin;
		double auto = block is Paragraph || block is List || block is Table ? lineHeight : 0;
		return new Thickness(Zero(m.Left), double.IsNaN(m.Top) ? auto : m.Top, Zero(m.Right),
			double.IsNaN(m.Bottom) ? auto : m.Bottom);
	}
}
