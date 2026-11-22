using System.Collections.Generic;

namespace System.Windows.Documents;

public partial class FlowDocument
{
	readonly List<TextEdit> _textEdits = new List<TextEdit>();

	/// <summary>A pointer at the start of the document content (logical direction Backward).</summary>
	public TextPointer ContentStart => new TextPointer(this, 0, LogicalDirection.Backward);

	/// <summary>A pointer at the end of the document content (logical direction Forward).</summary>
	public TextPointer ContentEnd =>
		new TextPointer(this, SymbolTotal, LogicalDirection.Forward);

	/// <summary>Number of edits made through the text-pointer API; pointers replay the ones they missed.</summary>
	internal int TextEditCount => _textEdits.Count;

	internal TextEdit GetTextEdit(int index) => _textEdits[index];

	/// <summary>Raised after an edit made through <see cref="TextPointer"/> or <see cref="TextRange"/>.</summary>
	internal event Action? ContentEdited;

	// Pointers re-anchor from this log instead of living in a text tree (the gap from WPF). An edit made by
	// changing Run.Text or a collection directly is not logged; pointers are then only clamped to the content
	// (TextPointer.Offset clamps to [0, total]).
	internal void RecordTextEdit(int position, int removed, int inserted)
	{
		_textEdits.Add(new TextEdit(position, removed, inserted));
		InvalidateSymbols();
		if (_editDepth == 0) ContentEdited?.Invoke();
	}

	int _editDepth;

	/// <summary>Runs several edits and raises <see cref="ContentEdited"/> once.</summary>
	internal void EditBatch(Action edit)
	{
		_editDepth++;
		try { edit(); }
		finally { _editDepth--; }
		if (_editDepth == 0) ContentEdited?.Invoke();
	}
}

/// <summary>One logged edit: <c>removed</c> symbols replaced by <c>inserted</c> ones at <c>Position</c>.</summary>
readonly struct TextEdit
{
	internal TextEdit(int position, int removed, int inserted)
	{
		Position = position;
		Removed = removed;
		Inserted = inserted;
	}

	internal int Position { get; }

	internal int Removed { get; }

	internal int Inserted { get; }
}
