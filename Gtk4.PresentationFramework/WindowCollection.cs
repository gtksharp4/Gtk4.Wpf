using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Windows;

/// <summary>A snapshot of the windows an <see cref="Application"/> owns.</summary>
public sealed class WindowCollection : ICollection
{
	readonly List<Window> _items;

	internal WindowCollection(IEnumerable<Window> windows)
	{
		_items = new List<Window>(windows);
	}

	/// <summary>Number of windows.</summary>
	public int Count => _items.Count;

	/// <summary>Always <see langword="false"/>; the collection is an immutable snapshot.</summary>
	public bool IsSynchronized => false;

	/// <summary>Object to lock on.</summary>
	public object SyncRoot => ((ICollection)_items).SyncRoot;

	/// <summary>Gets the window at <paramref name="index"/>.</summary>
	public Window this[int index] => _items[index];

	/// <summary>Copies the windows into <paramref name="array"/> starting at <paramref name="index"/>.</summary>
	public void CopyTo(Window[] array, int index) => _items.CopyTo(array, index);

	void ICollection.CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);

	/// <summary>Returns an enumerator over the windows.</summary>
	public IEnumerator GetEnumerator() => _items.GetEnumerator();
}
