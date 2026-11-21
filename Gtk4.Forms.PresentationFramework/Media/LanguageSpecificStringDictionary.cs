using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows.Media;

/// <summary>A string dictionary keyed by <see cref="XmlLanguage"/>, as used for font and face names.</summary>
public sealed class LanguageSpecificStringDictionary : IDictionary<XmlLanguage, string>
{
	readonly Dictionary<XmlLanguage, string> _items = new Dictionary<XmlLanguage, string>();
	bool _readOnly;

	internal LanguageSpecificStringDictionary()
	{
	}

	/// <summary>A read-only dictionary holding one entry.</summary>
	internal static LanguageSpecificStringDictionary Single(string language, string value)
	{
		var d = new LanguageSpecificStringDictionary();
		d._items[XmlLanguage.GetLanguage(language)] = value;
		d._readOnly = true;
		return d;
	}

	internal static LanguageSpecificStringDictionary ReadOnly()
	{
		return new LanguageSpecificStringDictionary { _readOnly = true };
	}

	void CheckWritable()
	{
		if (_readOnly) throw new NotSupportedException("The dictionary is read-only.");
	}

	/// <inheritdoc/>
	public string this[XmlLanguage key]
	{
		get
		{
			if (key == null) throw new ArgumentNullException(nameof(key));
			return _items[key];
		}
		set
		{
			CheckWritable();
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (value == null) throw new ArgumentNullException(nameof(value));
			_items[key] = value;
		}
	}

	/// <inheritdoc/>
	public int Count => _items.Count;

	/// <inheritdoc/>
	public bool IsReadOnly => _readOnly;

	/// <inheritdoc/>
	public ICollection<XmlLanguage> Keys => _items.Keys;

	/// <inheritdoc/>
	public ICollection<string> Values => _items.Values;

	/// <inheritdoc/>
	public void Add(XmlLanguage key, string value)
	{
		CheckWritable();
		if (key == null) throw new ArgumentNullException(nameof(key));
		if (value == null) throw new ArgumentNullException(nameof(value));
		_items.Add(key, value);
	}

	/// <inheritdoc/>
	public void Add(KeyValuePair<XmlLanguage, string> item) => Add(item.Key, item.Value);

	/// <inheritdoc/>
	public void Clear()
	{
		CheckWritable();
		_items.Clear();
	}

	/// <inheritdoc/>
	public bool Contains(KeyValuePair<XmlLanguage, string> item) =>
		item.Key != null && _items.TryGetValue(item.Key, out var v) && v == item.Value;

	/// <inheritdoc/>
	public bool ContainsKey(XmlLanguage key)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		return _items.ContainsKey(key);
	}

	/// <inheritdoc/>
	public void CopyTo(KeyValuePair<XmlLanguage, string>[] array, int arrayIndex)
	{
		if (array == null) throw new ArgumentNullException(nameof(array));
		if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
		if (array.Length - arrayIndex < _items.Count) throw new ArgumentException("The array is too small.", nameof(array));
		((ICollection<KeyValuePair<XmlLanguage, string>>)_items).CopyTo(array, arrayIndex);
	}

	/// <inheritdoc/>
	public bool Remove(XmlLanguage key)
	{
		CheckWritable();
		if (key == null) throw new ArgumentNullException(nameof(key));
		return _items.Remove(key);
	}

	/// <inheritdoc/>
	public bool Remove(KeyValuePair<XmlLanguage, string> item)
	{
		CheckWritable();
		return Contains(item) && _items.Remove(item.Key);
	}

	/// <inheritdoc/>
	public bool TryGetValue(XmlLanguage key, out string value)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		return _items.TryGetValue(key, out value!);
	}

	/// <inheritdoc/>
	public IEnumerator<KeyValuePair<XmlLanguage, string>> GetEnumerator() => _items.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
