using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Documents;

/// <summary>The document references of a <see cref="FixedDocumentSequence"/>.</summary>
public sealed class DocumentReferenceCollection : IList<DocumentReference>
{
	readonly List<DocumentReference> _items = new List<DocumentReference>();
	readonly FixedDocumentSequence _owner;

	internal DocumentReferenceCollection(FixedDocumentSequence owner) => _owner = owner;

	/// <summary>Number of references.</summary>
	public int Count => _items.Count;

	bool ICollection<DocumentReference>.IsReadOnly => false;

	/// <summary>The reference at <paramref name="index"/>.</summary>
	public DocumentReference this[int index]
	{
		get => _items[index];
		set
		{
			_items[index] = value ?? throw new ArgumentNullException(nameof(value));
			_owner.OnReferencesChanged();
		}
	}

	/// <summary>Appends a reference.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="item"/> is null.</exception>
	public void Add(DocumentReference item) => Insert(_items.Count, item);

	/// <summary>Inserts a reference.</summary>
	public void Insert(int index, DocumentReference item)
	{
		_items.Insert(index, item ?? throw new ArgumentNullException(nameof(item)));
		_owner.OnReferencesChanged();
	}

	/// <summary>Removes a reference.</summary>
	public bool Remove(DocumentReference item)
	{
		bool removed = _items.Remove(item);
		if (removed)
			_owner.OnReferencesChanged();
		return removed;
	}

	/// <summary>Removes the reference at <paramref name="index"/>.</summary>
	public void RemoveAt(int index)
	{
		_items.RemoveAt(index);
		_owner.OnReferencesChanged();
	}

	/// <summary>Removes all references.</summary>
	public void Clear()
	{
		_items.Clear();
		_owner.OnReferencesChanged();
	}

	/// <summary>Whether the reference is in the collection.</summary>
	public bool Contains(DocumentReference item) => _items.Contains(item);

	/// <summary>Index of the reference, or -1.</summary>
	public int IndexOf(DocumentReference item) => _items.IndexOf(item);

	/// <summary>Copies the references to an array.</summary>
	public void CopyTo(DocumentReference[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

	/// <inheritdoc/>
	public IEnumerator<DocumentReference> GetEnumerator() => _items.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
