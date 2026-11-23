using System.Collections.Generic;

namespace System.Windows.Documents;

/// <summary>Paginator of a <see cref="FixedDocumentSequence"/>: the pages of its documents in order.</summary>
sealed class FixedDocumentSequencePaginator : DocumentPaginator
{
	readonly FixedDocumentSequence _sequence;
	Size? _pageSize;

	internal FixedDocumentSequencePaginator(FixedDocumentSequence sequence) => _sequence = sequence;

	public override IDocumentPaginatorSource Source => _sequence;

	public override bool IsPageCountValid => true;

	public override int PageCount
	{
		get
		{
			int count = 0;
			foreach (var paginator in Paginators())
				count += paginator.PageCount;
			return count;
		}
	}

	/// <summary>The size set by the caller, else the size of the first page of the first document.</summary>
	public override Size PageSize
	{
		get
		{
			if (_pageSize.HasValue)
				return _pageSize.Value;
			foreach (var paginator in Paginators())
				if (paginator.PageCount > 0)
					return paginator.PageSize;
			return Size.Empty;
		}
		set => _pageSize = value;
	}

	/// <exception cref="ArgumentOutOfRangeException"><paramref name="pageNumber"/> is negative.</exception>
	/// <returns>The page, or <see cref="DocumentPage.Missing"/> past the last page.</returns>
	public override DocumentPage GetPage(int pageNumber)
	{
		if (pageNumber < 0)
			throw new ArgumentOutOfRangeException(nameof(pageNumber));
		int remaining = pageNumber;
		foreach (var paginator in Paginators())
		{
			if (remaining < paginator.PageCount)
				return paginator.GetPage(remaining);
			remaining -= paginator.PageCount;
		}
		return DocumentPage.Missing;
	}

	IEnumerable<DocumentPaginator> Paginators()
	{
		foreach (var reference in _sequence.References)
		{
			var document = reference.GetDocument(false);
			if (document != null)
				yield return document.DocumentPaginator;
		}
	}

	internal void NotifyPagesChanged() => OnPagesChanged(new PagesChangedEventArgs(0, PageCount));
}
