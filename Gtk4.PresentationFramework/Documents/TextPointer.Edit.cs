namespace System.Windows.Documents;

public sealed partial class TextPointer
{
	/// <summary>
	/// Inserts text at this position: into the Run there, else into a new Run (a new Paragraph between blocks).
	/// </summary>
	/// <exception cref="InvalidOperationException">The position cannot hold text.</exception>
	public void InsertTextInRun(string textData)
	{
		if (textData == null) throw new ArgumentNullException(nameof(textData));
		TextTreeEdit.InsertText(_doc, Offset, textData);
	}

	/// <summary>
	/// Deletes up to <paramref name="count"/> characters of the adjacent text run, backward when negative.
	/// </summary>
	/// <returns>The number of characters deleted.</returns>
	public int DeleteTextInRun(int count) => TextTreeEdit.DeleteInRun(_doc, Offset, count);
}
