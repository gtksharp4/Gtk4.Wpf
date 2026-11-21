using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Media;

/// <summary>The <see cref="FamilyTypeface"/> list of a <see cref="FontFamily"/>.</summary>
public sealed class FamilyTypefaceCollection : IList<FamilyTypeface>
{
	readonly List<FamilyTypeface> _items = new List<FamilyTypeface>();

	internal FamilyTypefaceCollection()
	{
	}

	/// <inheritdoc/>
	public FamilyTypeface this[int index]
	{
		get => _items[index];
		set => _items[index] = Check(value);
	}

	/// <inheritdoc/>
	public int Count => _items.Count;

	/// <inheritdoc/>
	public bool IsReadOnly => false;

	static FamilyTypeface Check(FamilyTypeface value) =>
		value ?? throw new ArgumentNullException(nameof(value));

	/// <inheritdoc/>
	public void Add(FamilyTypeface item) => _items.Add(Check(item));

	/// <inheritdoc/>
	public void Clear() => _items.Clear();

	/// <inheritdoc/>
	public bool Contains(FamilyTypeface item) => _items.Contains(item);

	/// <inheritdoc/>
	public void CopyTo(FamilyTypeface[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

	/// <inheritdoc/>
	public int IndexOf(FamilyTypeface item) => _items.IndexOf(item);

	/// <inheritdoc/>
	public void Insert(int index, FamilyTypeface item) => _items.Insert(index, Check(item));

	/// <inheritdoc/>
	public bool Remove(FamilyTypeface item) => _items.Remove(item);

	/// <inheritdoc/>
	public void RemoveAt(int index) => _items.RemoveAt(index);

	/// <inheritdoc/>
	public IEnumerator<FamilyTypeface> GetEnumerator() => _items.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
