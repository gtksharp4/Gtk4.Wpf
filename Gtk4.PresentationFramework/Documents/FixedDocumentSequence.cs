using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>
/// An ordered set of <see cref="FixedDocument"/>s referenced by <see cref="References"/>; its paginator chains
/// the pages of every document. Mirrors WPF <c>FixedDocumentSequence</c>.
/// </summary>
[ContentProperty("References")]
public class FixedDocumentSequence : FrameworkContentElement, IDocumentPaginatorSource, IAddChild, IUriContext
{
	FixedDocumentSequencePaginator? _paginator;
	Uri? _baseUri;

	/// <summary>Creates an empty sequence.</summary>
	public FixedDocumentSequence() => References = new DocumentReferenceCollection(this);

	/// <summary>The document references.</summary>
	public DocumentReferenceCollection References { get; }

	/// <summary>The paginator over every page of every referenced document.</summary>
	public DocumentPaginator DocumentPaginator => _paginator ??= new FixedDocumentSequencePaginator(this);

	Uri? IUriContext.BaseUri
	{
		get => _baseUri;
		set => _baseUri = value;
	}

	internal void OnReferencesChanged() => _paginator?.NotifyPagesChanged();

	void IAddChild.AddChild(object value)
	{
		if (value == null)
			throw new ArgumentNullException(nameof(value));
		if (!(value is DocumentReference reference))
			throw new ArgumentException(
				"A FixedDocumentSequence holds DocumentReference, not " + value.GetType().FullName + ".",
				nameof(value));
		References.Add(reference);
	}

	void IAddChild.AddText(string text)
	{
		if (!string.IsNullOrWhiteSpace(text))
			throw new ArgumentException("A FixedDocumentSequence does not accept text.", nameof(text));
	}
}
