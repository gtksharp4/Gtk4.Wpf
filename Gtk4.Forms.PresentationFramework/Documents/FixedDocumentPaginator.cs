namespace System.Windows.Documents;

/// <summary>Paginator of a <see cref="FixedDocument"/>: page n is the root of the n-th page.</summary>
sealed class FixedDocumentPaginator : DocumentPaginator
{
	readonly FixedDocument _document;
	Size? _pageSize;

	internal FixedDocumentPaginator(FixedDocument document) => _document = document;

	public override IDocumentPaginatorSource Source => _document;

	public override bool IsPageCountValid => true;

	public override int PageCount => _document.Pages.Count;

	/// <summary>The size set by the caller, else the size of the first page, else empty.</summary>
	public override Size PageSize
	{
		get
		{
			if (_pageSize.HasValue)
				return _pageSize.Value;
			return PageCount > 0 ? SizeOf(_document.Pages[0].GetPageRoot(false)) : Size.Empty;
		}
		set => _pageSize = value;
	}

	/// <exception cref="ArgumentOutOfRangeException"><paramref name="pageNumber"/> is negative.</exception>
	/// <returns>The page, or <see cref="DocumentPage.Missing"/> past the last page.</returns>
	public override DocumentPage GetPage(int pageNumber)
	{
		if (pageNumber < 0)
			throw new ArgumentOutOfRangeException(nameof(pageNumber));
		if (pageNumber >= PageCount)
			return DocumentPage.Missing;
		var root = _document.Pages[pageNumber].GetPageRoot(false);
		if (root == null)
			return DocumentPage.Missing;
		var size = SizeOf(root);
		var box = new Rect(0, 0, size.Width, size.Height);
		return new DocumentPage(root, size, box, box);
	}

	internal void NotifyPagesChanged() => OnPagesChanged(new PagesChangedEventArgs(0, PageCount));

	static Size SizeOf(FixedPage? page) =>
		page == null ? Size.Empty : new Size(double.IsNaN(page.Width) ? 0 : page.Width,
			double.IsNaN(page.Height) ? 0 : page.Height);
}
