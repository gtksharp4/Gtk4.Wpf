using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>
/// A block element that groups other blocks.
/// Renders as its child blocks concatenated (with blank-line separators).
/// </summary>
[ContentProperty("Blocks")]
public class Section : Block
{
	/// <summary>Child blocks in this section.</summary>
	public BlockCollection Blocks { get; } = new BlockCollection();

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
	internal override void AppendMarkup(System.Text.StringBuilder sb)
	{
		bool first = true;
		foreach (var block in Blocks)
		{
			if (!first) sb.Append('\n');
			first = false;
			block.AppendMarkup(sb);
		}
	}
}