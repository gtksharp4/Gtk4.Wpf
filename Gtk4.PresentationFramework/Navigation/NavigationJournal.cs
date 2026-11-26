using System;
using System.Collections.Generic;

namespace System.Windows.Navigation;

/// <summary>
/// Pure back/forward journal for <see cref="NavigationService"/>.
/// The cursor points at the current entry; entries after it are the forward stack.
/// </summary>
internal sealed class NavigationJournal
{
	readonly List<object?> _entries = new List<object?>();
	int _cursor = -1; // index of current entry; -1 = empty

	/// <summary>True when <see cref="GoBack"/> is possible.</summary>
	public bool CanGoBack => _cursor > 0;

	/// <summary>True when <see cref="GoForward"/> is possible.</summary>
	public bool CanGoForward => _cursor < _entries.Count - 1;

	/// <summary>The currently active content (may be null for a fresh empty journal).</summary>
	public object? Current => _cursor >= 0 ? _entries[_cursor] : null;

	/// <summary>
	/// Adds <paramref name="content"/> as a new entry. Any entries forward of the
	/// cursor are discarded (matches WPF journal semantics).
	/// </summary>
	public void Push(object? content)
	{
		// Trim forward entries.
		if (_cursor < _entries.Count - 1)
			_entries.RemoveRange(_cursor + 1, _entries.Count - _cursor - 1);

		_entries.Add(content);
		_cursor = _entries.Count - 1;
	}

	/// <summary>Moves the cursor back one entry and returns the new current content.</summary>
	/// <exception cref="InvalidOperationException">If <see cref="CanGoBack"/> is false.</exception>
	public object? GoBack()
	{
		if (!CanGoBack)
			throw new InvalidOperationException("No back entries.");
		_cursor--;
		return Current;
	}

	/// <summary>Moves the cursor forward one entry and returns the new current content.</summary>
	/// <exception cref="InvalidOperationException">If <see cref="CanGoForward"/> is false.</exception>
	public object? GoForward()
	{
		if (!CanGoForward)
			throw new InvalidOperationException("No forward entries.");
		_cursor++;
		return Current;
	}

	/// <summary>Drops the current entry and moves the cursor to the previous one.</summary>
	public void RemoveCurrent()
	{
		if (_cursor < 0) return;
		_entries.RemoveAt(_cursor);
		_cursor--;
	}

	/// <summary>Replaces the current entry with <paramref name="content"/>.</summary>
	public void ReplaceCurrent(object? content)
	{
		if (_cursor >= 0) _entries[_cursor] = content;
	}

	/// <summary>Whether <paramref name="content"/> is any entry, by reference.</summary>
	public bool Contains(object content)
	{
		foreach (var entry in _entries)
			if (ReferenceEquals(entry, content)) return true;
		return false;
	}

	/// <summary>Clears all entries.</summary>
	public void Clear()
	{
		_entries.Clear();
		_cursor = -1;
	}
}