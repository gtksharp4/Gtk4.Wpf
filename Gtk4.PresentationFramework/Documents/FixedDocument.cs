using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>
/// A document made of fixed pages. Mirrors WPF <c>FixedDocument</c>; the paginator hands out the page roots
/// (<see cref="FixedPage"/>) of <see cref="Pages"/>.
/// </summary>
[ContentProperty("Pages")]
public class FixedDocument : FrameworkContentElement, IDocumentPaginatorSource, IAddChild, IUriContext
{
	FixedDocumentPaginator? _paginator;
	Uri? _baseUri;

	/// <summary>Creates an empty document.</summary>
	public FixedDocument() => Pages = new PageContentCollection(this);

	/// <summary>The pages.</summary>
	public PageContentCollection Pages { get; }

	/// <summary>The paginator over <see cref="Pages"/>.</summary>
	public DocumentPaginator DocumentPaginator => _paginator ??= new FixedDocumentPaginator(this);

	Uri? IUriContext.BaseUri
	{
		get => _baseUri;
		set => _baseUri = value;
	}

	internal void OnPagesChanged() => _paginator?.NotifyPagesChanged();

	void IAddChild.AddChild(object value)
	{
		if (value == null)
			throw new ArgumentNullException(nameof(value));
		if (!(value is PageContent page))
			throw new ArgumentException("A FixedDocument holds PageContent, not " + value.GetType().FullName + ".",
				nameof(value));
		Pages.Add(page);
	}

	void IAddChild.AddText(string text)
	{
		if (!string.IsNullOrWhiteSpace(text))
			throw new ArgumentException("A FixedDocument does not accept text.", nameof(text));
	}
}
