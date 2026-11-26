namespace System.Windows.Documents;

/// <summary>
/// Pure, GTK-free helper that converts a <see cref="TextDecorationCollection"/>
/// into Pango markup attribute fragments.
/// </summary>
internal static class TextDecorationMarkup
{
	/// <summary>
	/// Returns the Pango span attributes string (without enclosing <c>&lt;span&gt;</c> tags)
	/// that represents the given <paramref name="decorations"/>.
	/// Returns an empty string when <paramref name="decorations"/> is <c>null</c> or empty.
	/// </summary>
	/// <remarks>
	/// Mapping:
	/// <list type="bullet">
	///   <item><see cref="TextDecorationLocation.Underline"/> → <c>underline="single"</c></item>
	///   <item><see cref="TextDecorationLocation.Strikethrough"/> → <c>strikethrough="true"</c></item>
	///   <item><see cref="TextDecorationLocation.OverLine"/> → <c>overline="single"</c>
	///     (Pango ≥ 1.46 / GTK ≥ 4.0; silently ignored on older runtimes)</item>
	///   <item><see cref="TextDecorationLocation.Baseline"/> → <c>underline="single"</c>
	///     (closest Pango approximation)</item>
	/// </list>
	/// When both Underline and Baseline are present only one <c>underline="single"</c> is emitted.
	/// </remarks>
	internal static string ToPangoAttributes(TextDecorationCollection? decorations)
	{
		if (decorations == null || decorations.Count == 0)
			return string.Empty;

		bool underline = false;
		bool strikethrough = false;
		bool overline = false;

		foreach (var dec in decorations)
		{
			switch (dec.Location)
			{
				case TextDecorationLocation.Underline:
				case TextDecorationLocation.Baseline:
					underline = true;
					break;
				case TextDecorationLocation.Strikethrough:
					strikethrough = true;
					break;
				case TextDecorationLocation.OverLine:
					overline = true;
					break;
			}
		}

		if (!underline && !strikethrough && !overline)
			return string.Empty;

		var sb = new System.Text.StringBuilder();
		if (underline) sb.Append(" underline=\"single\"");
		if (strikethrough) sb.Append(" strikethrough=\"true\"");
		if (overline) sb.Append(" overline=\"single\"");
		return sb.ToString().TrimStart();
	}

	/// <summary>
	/// Wraps the text content in a <c>&lt;span&gt;</c> element carrying the decoration
	/// attributes, or returns the content unchanged when there are no decorations.
	/// </summary>
	/// <param name="decorations">The decorations to apply, or <c>null</c>.</param>
	/// <param name="content">The already-escaped / markup-safe inner content.</param>
	internal static string WrapWithDecorations(TextDecorationCollection? decorations, string content)
	{
		string attrs = ToPangoAttributes(decorations);
		if (attrs.Length == 0)
			return content;
		return $"<span {attrs}>{content}</span>";
	}
}
