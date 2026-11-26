namespace System.Windows.Controls;

public partial class TextBox
{
	/// <summary>Copies the selected text to the clipboard; does nothing when nothing is selected.</summary>
	public void Copy()
	{
		PullSelectionFromWidget();
		if (SelectionLength > 0)
			System.Windows.Clipboard.SetText(SelectedText);
	}

	/// <summary>Moves the selected text to the clipboard; does nothing when nothing is selected or read-only.</summary>
	public void Cut()
	{
		PullSelectionFromWidget();
		if (SelectionLength == 0 || IsReadOnly) return;
		System.Windows.Clipboard.SetText(SelectedText);
		SelectedText = string.Empty;
	}

	/// <summary>Replaces the selection with the clipboard text; does nothing when empty or read-only.</summary>
	public void Paste()
	{
		if (IsReadOnly || !System.Windows.Clipboard.ContainsText()) return;
		PullSelectionFromWidget();
		SelectedText = System.Windows.Clipboard.GetText();
	}

	// The user selects and moves the caret in the GTK widget, so it, not the stored range, is current.
	void PullSelectionFromWidget()
	{
		if (!IsWidgetCreated) return;
		if (_entry != null)
		{
			if (_entry.GetSelectionBounds(out int start, out int end))
			{
				_selectionStart = start;
				_selectionLength = end - start;
			}
			else
			{
				_selectionStart = _entry.Position;
				_selectionLength = 0;
			}
		}
		else if (_textView != null)
		{
			var buffer = _textView.Buffer;
			if (buffer.GetSelectionBounds(out var startIter, out var endIter))
			{
				_selectionStart = startIter.Offset;
				_selectionLength = endIter.Offset - startIter.Offset;
			}
			else
			{
				_selectionStart = buffer.GetIterAtMark(buffer.InsertMark).Offset;
				_selectionLength = 0;
			}
		}
		ClampSelectionToTextLength();
	}
}
