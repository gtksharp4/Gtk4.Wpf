using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows.Media;

/// <summary>Maps Unicode ranges to a target family for composite fonts. Stored only: GTK does no composition.</summary>
public sealed class FontFamilyMap
{
	/// <summary>Space-separated Unicode ranges such as <c>0000-00FF</c>.</summary>
	public string? Unicode { get; set; }

	/// <summary>The target family name.</summary>
	public string? Target { get; set; }

	/// <summary>The size scale applied to the target (default 1).</summary>
	public double Scale { get; set; } = 1.0;

	/// <summary>The language the mapping applies to (default empty: all).</summary>
	public XmlLanguage Language { get; set; } = XmlLanguage.Empty;
}

/// <summary>The <see cref="FontFamilyMap"/> list of a <see cref="FontFamily"/>.</summary>
public sealed class FontFamilyMapCollection : IList<FontFamilyMap>
{
	readonly List<FontFamilyMap> _items = new List<FontFamilyMap>();

	internal FontFamilyMapCollection()
	{
	}

	/// <inheritdoc/>
	public FontFamilyMap this[int index]
	{
		get => _items[index];
		set => _items[index] = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <inheritdoc/>
	public int Count => _items.Count;

	/// <inheritdoc/>
	public bool IsReadOnly => false;

	/// <inheritdoc/>
	public void Add(FontFamilyMap item) => _items.Add(item ?? throw new ArgumentNullException(nameof(item)));

	/// <inheritdoc/>
	public void Clear() => _items.Clear();

	/// <inheritdoc/>
	public bool Contains(FontFamilyMap item) => _items.Contains(item);

	/// <inheritdoc/>
	public void CopyTo(FontFamilyMap[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

	/// <inheritdoc/>
	public int IndexOf(FontFamilyMap item) => _items.IndexOf(item);

	/// <inheritdoc/>
	public void Insert(int index, FontFamilyMap item) =>
		_items.Insert(index, item ?? throw new ArgumentNullException(nameof(item)));

	/// <inheritdoc/>
	public bool Remove(FontFamilyMap item) => _items.Remove(item);

	/// <inheritdoc/>
	public void RemoveAt(int index) => _items.RemoveAt(index);

	/// <inheritdoc/>
	public IEnumerator<FontFamilyMap> GetEnumerator() => _items.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
