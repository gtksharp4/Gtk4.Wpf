using System;
using System.Collections;

namespace System.Windows;

/// <summary>
/// Enumerates the local values of a <see cref="DependencyObject"/>. It works on a snapshot taken when
/// <see cref="DependencyObject.GetLocalValueEnumerator"/> is called. Mirrors <c>LocalValueEnumerator</c>.
/// </summary>
public struct LocalValueEnumerator : IEnumerator
{
	readonly LocalValueEntry[] _entries;
	int _index;

	internal LocalValueEnumerator(LocalValueEntry[] entries)
	{
		_entries = entries;
		_index = -1;
	}

	/// <summary>The number of local values in the snapshot.</summary>
	public int Count => _entries?.Length ?? 0;

	/// <summary>The current entry. Throws <see cref="InvalidOperationException"/> outside the snapshot.</summary>
	public LocalValueEntry Current
	{
		get
		{
			if (_index < 0 || _index >= Count)
				throw new InvalidOperationException("The enumerator is not positioned on an entry.");
			return _entries[_index];
		}
	}

	object IEnumerator.Current => Current;

	/// <summary>Advances to the next entry.</summary>
	public bool MoveNext()
	{
		if (_index < Count) _index++;
		return _index < Count;
	}

	/// <summary>Resets the enumerator to before the first entry.</summary>
	public void Reset() => _index = -1;

	/// <summary>Compares two enumerators for equality.</summary>
	public static bool operator ==(LocalValueEnumerator obj1, LocalValueEnumerator obj2) => obj1.Equals(obj2);

	/// <summary>Compares two enumerators for inequality.</summary>
	public static bool operator !=(LocalValueEnumerator obj1, LocalValueEnumerator obj2) => !obj1.Equals(obj2);

	/// <inheritdoc/>
	public override bool Equals(object? obj) =>
		obj is LocalValueEnumerator e && ReferenceEquals(_entries, e._entries) && _index == e._index;

	/// <inheritdoc/>
	public override int GetHashCode() => (_entries?.GetHashCode() ?? 0) ^ _index;
}
