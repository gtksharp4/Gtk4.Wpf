namespace System.Windows;

/// <summary>Specifies how overflowing text is trimmed.</summary>
public enum TextTrimming
{
	/// <summary>No trimming; text is clipped at the content edge.</summary>
	None,
	/// <summary>
	/// Text is trimmed at a character boundary and an ellipsis (<c>…</c>) is inserted.
	/// Pango maps this to <c>EllipsizeMode.End</c> (no word/character distinction).
	/// </summary>
	CharacterEllipsis,
	/// <summary>
	/// Text is trimmed at a word boundary and an ellipsis (<c>…</c>) is inserted.
	/// Pango maps this to <c>EllipsizeMode.End</c> — identical to
	/// <see cref="CharacterEllipsis"/> at the GTK layer.
	/// </summary>
	WordEllipsis,
}
