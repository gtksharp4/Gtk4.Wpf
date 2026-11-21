using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>
/// An item within a <see cref="List"/>. Contains block content.
/// </summary>
[ContentProperty("Blocks")]
public class ListItem
{
	/// <summary>Block content of this list item.</summary>
	public BlockCollection Blocks { get; } = new BlockCollection();

	FlowDirection? _flowDirection;

	/// <summary>Gets or sets the flow direction of this item (default LeftToRight).</summary>
	public FlowDirection FlowDirection
	{
		get => _flowDirection ?? FlowDirection.LeftToRight;
		set
		{
			if (!FrameworkElement.IsValidFlowDirection(value))
				throw new ArgumentException($"'{value}' is not a valid value for property 'FlowDirection'.");
			_flowDirection = value;
		}
	}

	internal FlowDirection? ExplicitFlowDirection => _flowDirection;

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