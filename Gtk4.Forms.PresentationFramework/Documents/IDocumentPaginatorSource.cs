namespace System.Windows.Documents;

/// <summary>Implemented by content that can be paginated.</summary>
public interface IDocumentPaginatorSource
{
	/// <summary>Gets the paginator that lays the content out in pages.</summary>
	DocumentPaginator DocumentPaginator { get; }
}
