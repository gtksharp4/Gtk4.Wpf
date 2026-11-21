namespace System.Windows.Documents;

/// <summary>Describes what lies next to a <see cref="TextPointer"/> in a given direction.</summary>
public enum TextPointerContext
{
	/// <summary>There is no content: the pointer is at the edge of the document.</summary>
	None = 0,

	/// <summary>Text characters.</summary>
	Text = 1,

	/// <summary>An embedded UI element (<see cref="InlineUIContainer"/> or <see cref="BlockUIContainer"/> child).</summary>
	EmbeddedElement = 2,

	/// <summary>The start edge of an element.</summary>
	ElementStart = 3,

	/// <summary>The end edge of an element.</summary>
	ElementEnd = 4,
}
