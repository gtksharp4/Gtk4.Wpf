using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>
/// A block element that groups other blocks.
/// Renders as its child blocks concatenated (with blank-line separators).
/// </summary>
[ContentProperty("Blocks")]
public partial class Section : Block
{
	/// <summary>Child blocks in this section.</summary>
	public BlockCollection Blocks { get; }

	/// <summary>Creates an empty section.</summary>
	public Section() => Blocks = new BlockCollection(this);

	/// <inheritdoc/>
	internal override void AppendPlainText(System.Text.StringBuilder sb)
	{
		bool first = true;
		foreach (var block in Blocks)
		{
			if (!first) sb.Append("\n\n");
			first = false;
			block.AppendPlainText(sb);
		}
	}

	/// <inheritdoc/>
	internal override void AppendMarkup(System.Text.StringBuilder sb) => AppendBlocks(sb, null);

	/// <inheritdoc/>
	internal override void AppendFlowMarkup(System.Text.StringBuilder sb, FlowDirection? effective) =>
		AppendLanguage(sb, s => AppendBlocks(s, effective));

	// Each child block opens and closes its own isolate: a container's isolate would end at the first separator.
	void AppendBlocks(System.Text.StringBuilder sb, FlowDirection? effective)
	{
		bool first = true;
		foreach (var block in Blocks)
		{
			if (!first) sb.Append('\n');
			first = false;
			block.AppendBlockMarkup(sb, effective);
		}
	}
}
