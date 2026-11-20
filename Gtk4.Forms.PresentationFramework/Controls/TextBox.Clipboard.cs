namespace System.Windows.Controls;

public partial class TextBox
{
	/// <summary>Copies the selected text to the clipboard; does nothing when nothing is selected.</summary>
	public void Copy()
	{
		if (SelectionLength > 0)
			System.Windows.Clipboard.SetText(SelectedText);
	}

	/// <summary>Moves the selected text to the clipboard; does nothing when nothing is selected or read-only.</summary>
	public void Cut()
	{
		if (SelectionLength == 0 || IsReadOnly) return;
		System.Windows.Clipboard.SetText(SelectedText);
		SelectedText = string.Empty;
	}

	/// <summary>Replaces the selection with the clipboard text; does nothing when empty or read-only.</summary>
	public void Paste()
	{
		if (IsReadOnly || !System.Windows.Clipboard.ContainsText()) return;
		SelectedText = System.Windows.Clipboard.GetText();
	}
}
