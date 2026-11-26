namespace System.Windows.Documents;

/// <summary>
/// Pagination of a flow document. The paginator is a <see cref="DynamicDocumentPaginator"/> whose page count is an
/// approximation (see <see cref="FlowDocumentPageMeter"/>), not WPF's layout; the pages are text-only drawing visuals.
/// </summary>
public partial class FlowDocument : IDocumentPaginatorSource
{
	DocumentPaginator? _paginator;

	/// <inheritdoc/>
	DocumentPaginator IDocumentPaginatorSource.DocumentPaginator => _paginator ??= new FlowDocumentPaginator(this);
}
