using System;

namespace System.Windows.Controls;

/// <summary>
/// Selection model for <see cref="TextBox"/>: SelectionStart, SelectionLength,
/// SelectedText, CaretIndex, Select, SelectAll, Clear.
/// </summary>
public partial class TextBox
{
	int _selectionStart;
	int _selectionLength;

	/// <summary>Gets or sets the character index of the selection start.</summary>
	public int SelectionStart
	{
		get => _selectionStart;
		set
		{
			value = Math.Max(0, Math.Min(value, Text.Length));
			if (_selectionStart == value) return;
			_selectionStart = value;
			// Re-clamp SelectionLength so start+length never exceeds text end.
			_selectionLength = Math.Max(0, Math.Min(_selectionLength, Text.Length - _selectionStart));
			SyncSelectionToWidget();
			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	/// <summary>Gets or sets the selection length in characters.</summary>
	public int SelectionLength
	{
		get => _selectionLength;
		set
		{
			value = Math.Max(0, Math.Min(value, Text.Length - _selectionStart));
			if (_selectionLength == value) return;
			_selectionLength = value;
			SyncSelectionToWidget();
			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	/// <summary>Gets or sets the currently selected text.</summary>
	public string SelectedText
	{
		get
		{
			if (_selectionLength == 0) return string.Empty;
			return Text.Substring(
				Math.Min(_selectionStart, Text.Length),
				Math.Min(_selectionLength, Text.Length - _selectionStart));
		}
		set
		{
			if (value == null) value = string.Empty;
			var current = Text;
			var before = current.Substring(0, _selectionStart);
			var after  = current.Substring(_selectionStart + _selectionLength);
			// Caret lands at end of inserted text, selection collapses (WPF behaviour).
			var insertionPoint = _selectionStart + value.Length;
			SetValue(TextProperty, before + value + after);
			_selectionStart  = insertionPoint;
			_selectionLength = 0;
			SyncSelectionToWidget();
			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	// ── CaretIndex ───────────────────────────────────────────────────────────

	/// <summary>Gets or sets the caret position (character index).</summary>
	public int CaretIndex
	{
		get => _selectionStart;
		set
		{
			value = Math.Max(0, Math.Min(value, Text.Length));
			if (_selectionStart == value) return;
			_selectionStart  = value;
			_selectionLength = 0;
			SyncSelectionToWidget();
		}
	}

	// ── Methods ──────────────────────────────────────────────────────────────

	/// <summary>Selects the range from <paramref name="start"/> for <paramref name="length"/> characters.</summary>
	public void Select(int start, int length)
	{
		start  = Math.Max(0, Math.Min(start, Text.Length));
		length = Math.Max(0, Math.Min(length, Text.Length - start));
		_selectionStart  = start;
		_selectionLength = length;
		SyncSelectionToWidget();
		SelectionChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>Selects all text.</summary>
	public void SelectAll()
	{
		_selectionStart  = 0;
		_selectionLength = Text.Length;
		SyncSelectionToWidget();
		SelectionChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>Clears the text.</summary>
	public void Clear()
	{
		SetValue(TextProperty, string.Empty);
		_selectionStart  = 0;
		_selectionLength = 0;
	}
}