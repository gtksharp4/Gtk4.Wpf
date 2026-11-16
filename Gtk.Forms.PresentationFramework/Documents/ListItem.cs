using System.Windows.Markup;

namespace System.Windows.Documents
{
	/// <summary>
	/// An item within a <see cref="List"/>. Contains block content.
	/// </summary>
	[ContentProperty("Blocks")]
	public class ListItem
	{
		/// <summary>Block content of this list item.</summary>
		public BlockCollection Blocks { get; } = new BlockCollection();

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
				block.AppendMarkup(sb);
			}
		}
	}
}
