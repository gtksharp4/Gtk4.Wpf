using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>
/// An item within a <see cref="List"/>. Contains block content.
/// </summary>
[ContentProperty("Blocks")]
public class ListItem : TextElement
{
	/// <summary>Creates an empty list item.</summary>
	public ListItem() => Blocks = new BlockCollection(this);

	/// <summary>Block content of this list item.</summary>
	public BlockCollection Blocks { get; }

	/// <summary>Gets or sets the flow direction of this item (default LeftToRight).</summary>
	public FlowDirection FlowDirection
	{
		get => GetFlowDirection();
		set => SetFlowDirection(value);
	}


	/// <summary>Builds the plain-text for this item (without the marker prefix).</summary>
	internal void AppendPlainText(System.Text.StringBuilder sb)
	{
		bool first = true;
		foreach (var block in Blocks)
		{
			if (!first) sb.Append(' ');
			first = false;
			block.AppendPlainText(sb);
		}
	}

	/// <summary>Builds the Pango markup for this item (without the marker prefix).</summary>
	internal void AppendMarkup(System.Text.StringBuilder sb)
	{
		bool first = true;
		foreach (var block in Blocks)
		{
			if (!first) sb.Append(' ');
			first = false;
			block.AppendWrappedMarkup(sb);
		}
	}
}