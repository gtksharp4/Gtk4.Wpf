namespace System.Windows.Documents;

/// <summary>
/// A block element that contains a sequence of <see cref="Inline"/> elements.
/// Renders as a paragraph separated from adjacent blocks by blank lines.
/// </summary>
public partial class Paragraph : Block
{
	/// <summary>The inline content of this paragraph.</summary>
	public InlineCollection Inlines { get; }

	/// <summary>Creates an empty paragraph.</summary>
	public Paragraph() => Inlines = new InlineCollection(this);

	/// <summary>Creates a paragraph holding <paramref name="inline"/>.</summary>
	public Paragraph(Inline inline) : this()
	{
		if (inline == null) throw new ArgumentNullException(nameof(inline));
		Inlines.Add(inline);
	}

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
			inline.AppendWrappedMarkup(sb);
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