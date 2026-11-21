using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Annotations;

/// <summary>The name/value dictionary of a <see cref="ContentLocatorPart"/>; reports every mutation.</summary>
sealed class NotifyingDictionary : IDictionary<string, string>
{
	readonly Dictionary<string, string> _items = new Dictionary<string, string>();

	internal event Action? Changed;

	public string this[string key]
	{
		get => _items[key];
		set
		{
			Check(key, value);
			_items[key] = value;
			Changed?.Invoke();
		}
	}

	public ICollection<string> Keys => _items.Keys;

	public ICollection<string> Values => _items.Values;

	public int Count => _items.Count;

	public bool IsReadOnly => false;

	public void Add(string key, string value)
	{
		Check(key, value);
		_items.Add(key, value);
		Changed?.Invoke();
	}

	public void Add(KeyValuePair<string, string> item) => Add(item.Key, item.Value);

	public void Clear()
	{
		if (_items.Count == 0) return;
		_items.Clear();
		Changed?.Invoke();
	}

	public bool Contains(KeyValuePair<string, string> item) =>
		_items.TryGetValue(item.Key, out var v) && v == item.Value;

	public bool ContainsKey(string key) => _items.ContainsKey(key);

	public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) =>
		((ICollection<KeyValuePair<string, string>>)_items).CopyTo(array, arrayIndex);

	public bool Remove(string key)
	{
		if (!_items.Remove(key)) return false;
		Changed?.Invoke();
		return true;
	}

	public bool Remove(KeyValuePair<string, string> item) => Contains(item) && Remove(item.Key);

	public bool TryGetValue(string key, out string value) => _items.TryGetValue(key, out value!);

	public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _items.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	static void Check(string key, string value)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		if (value == null) throw new ArgumentNullException(nameof(value));
	}
}
