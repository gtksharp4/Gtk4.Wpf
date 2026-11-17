using System;
using System.Collections.Generic;

namespace System.Windows.Controls;

public partial class WebBrowser
{
	// ── Back/forward journal ──────────────────────────────────────────────────

	readonly List<Uri> _history = new List<Uri>();
	int _cursor = -1; // index of the current entry in _history

	/// <summary>
	/// Pushes <paramref name="uri"/> onto the history list.
	/// Any forward entries beyond the current cursor are trimmed first.
	/// </summary>
	void HistoryPush(Uri uri)
	{
		// Trim forward history (new navigation discards it).
		if (_cursor < _history.Count - 1)
			_history.RemoveRange(_cursor + 1, _history.Count - _cursor - 1);

		_history.Add(uri);
		_cursor = _history.Count - 1;
		UpdateJournalDPs();
	}

	/// <summary>
	/// Moves the cursor one step back and returns the entry.
	/// Caller must guard with <see cref="CanGoBack"/>.
	/// </summary>
	Uri HistoryGoBack()
	{
		_cursor--;
		UpdateJournalDPs();
		return _history[_cursor];
	}

	/// <summary>
	/// Moves the cursor one step forward and returns the entry.
	/// Caller must guard with <see cref="CanGoForward"/>.
	/// </summary>
	Uri HistoryGoForward()
	{
		_cursor++;
		UpdateJournalDPs();
		return _history[_cursor];
	}

	/// <summary>Returns the entry at the current cursor, or <c>null</c> if the history is empty.</summary>
	Uri? HistoryCurrent() =>
		_cursor >= 0 && _cursor < _history.Count ? _history[_cursor] : null;

	/// <summary>Returns the entry one step back without moving the cursor, or <c>null</c>.</summary>
	Uri? PeekBack() =>
		_cursor > 0 ? _history[_cursor - 1] : null;

	/// <summary>Returns the entry one step forward without moving the cursor, or <c>null</c>.</summary>
	Uri? PeekForward() =>
		_cursor < _history.Count - 1 ? _history[_cursor + 1] : null;

	void UpdateJournalDPs()
	{
		SetValue(CanGoBackPropertyKey, _cursor > 0);
		SetValue(CanGoForwardPropertyKey, _cursor < _history.Count - 1);
	}
}