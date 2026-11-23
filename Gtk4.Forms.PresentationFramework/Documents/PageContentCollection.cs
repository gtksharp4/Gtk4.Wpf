using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Documents;

/// <summary>The pages of a <see cref="FixedDocument"/>. Mirrors WPF <c>PageContentCollection</c>.</summary>
public sealed class PageContentCollection : IEnumerable<PageContent>
{
	readonly List<PageContent> _items = new List<PageContent>();
	readonly FixedDocument _owner;

	internal PageContentCollection(FixedDocument owner) => _owner = owner;

	/// <summary>Number of pages.</summary>
	public int Count => _items.Count;

	/// <summary>The page at <paramref name="index"/>.</summary>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the collection.</exception>
	public PageContent this[int index] => _items[index];

	/// <summary>Appends a page and returns its index.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="newPageContent"/> is null.</exception>
	public int Add(PageContent newPageContent)
	{
		if (newPageContent == null)
			throw new ArgumentNullException(nameof(newPageContent));
		_items.Add(newPageContent);
		_owner.OnPagesChanged();
		return _items.Count - 1;
	}

	/// <inheritdoc/>
	public IEnumerator<PageContent> GetEnumerator() => _items.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
