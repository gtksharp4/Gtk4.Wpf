namespace System.Windows.Documents;

/// <summary>
/// A block element that contains a sequence of <see cref="Inline"/> elements.
/// Renders as a paragraph separated from adjacent blocks by blank lines.
/// </summary>
public class Paragraph : Block
{
	/// <summary>The inline content of this paragraph.</summary>
	public InlineCollection Inlines { get; } = new InlineCollection();

	/// <inheritdoc/>
	internal override void AppendPlainText(System.Text.StringBuilder sb)
	{
		foreach (var inline in Inlines)
			AppendInlinePlainText(inline, sb);
	}

	/// <inheritdoc/>
	internal override void AppendMarkup(System.Text.StringBuilder sb)
	{
		foreach (var inline in Inlines)
			inline.AppendMarkup(sb);
	}

	static void AppendInlinePlainText(Inline inline, System.Text.StringBuilder sb)
	{
		switch (inline)
		{
			case Run r:
				sb.Append(r.Text);
				break;
			case LineBreak _:
				sb.Append('\n');
				break;
			case Span s:
				foreach (var child in s.Inlines)
					AppendInlinePlainText(child, sb);
				break;
		}
	}
}