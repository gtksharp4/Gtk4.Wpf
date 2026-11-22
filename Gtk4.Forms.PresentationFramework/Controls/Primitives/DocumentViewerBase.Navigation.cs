using System.Collections.ObjectModel;

namespace System.Windows.Controls.Primitives;

public abstract partial class DocumentViewerBase
{
	static readonly DependencyPropertyKey PageCountPropertyKey = DependencyProperty.RegisterReadOnly(
		nameof(PageCount), typeof(int), typeof(DocumentViewerBase), new PropertyMetadata(0));

	static readonly DependencyPropertyKey MasterPageNumberPropertyKey = DependencyProperty.RegisterReadOnly(
		nameof(MasterPageNumber), typeof(int), typeof(DocumentViewerBase), new PropertyMetadata(0));

	static readonly DependencyPropertyKey CanGoToPreviousPagePropertyKey = DependencyProperty.RegisterReadOnly(
		nameof(CanGoToPreviousPage), typeof(bool), typeof(DocumentViewerBase), new PropertyMetadata(false));

	static readonly DependencyPropertyKey CanGoToNextPagePropertyKey = DependencyProperty.RegisterReadOnly(
		nameof(CanGoToNextPage), typeof(bool), typeof(DocumentViewerBase), new PropertyMetadata(false));

	/// <summary>Identifies the PageCount property.</summary>
	public static readonly DependencyProperty PageCountProperty = PageCountPropertyKey.DependencyProperty;

	/// <summary>Identifies the MasterPageNumber property.</summary>
	public static readonly DependencyProperty MasterPageNumberProperty = MasterPageNumberPropertyKey.DependencyProperty;

	/// <summary>Identifies the CanGoToPreviousPage property.</summary>
	public static readonly DependencyProperty CanGoToPreviousPageProperty =
		CanGoToPreviousPagePropertyKey.DependencyProperty;

	/// <summary>Identifies the CanGoToNextPage property.</summary>
	public static readonly DependencyProperty CanGoToNextPageProperty = CanGoToNextPagePropertyKey.DependencyProperty;

	DocumentPageView[]? _views;

	/// <summary>Gets the number of pages in <see cref="Document"/>; 0 without one.</summary>
	public int PageCount => (int)GetValue(PageCountProperty)!;

	/// <summary>Gets the one-based number of the page shown first; 0 without a document.</summary>
	public int MasterPageNumber => (int)GetValue(MasterPageNumberProperty)!;

	/// <summary>Gets whether a page precedes the current one.</summary>
	public bool CanGoToPreviousPage => (bool)GetValue(CanGoToPreviousPageProperty)!;

	/// <summary>Gets whether a page follows the current one.</summary>
	public bool CanGoToNextPage => (bool)GetValue(CanGoToNextPageProperty)!;

	/// <summary>Gets the page views that show the document; one view for the base viewers.</summary>
	public ReadOnlyCollection<DocumentPageView> PageViews
	{
		get
		{
			if (_views == null)
			{
				_views = new[] { new DocumentPageView() };
				SyncViews();
			}
			return new ReadOnlyCollection<DocumentPageView>(_views);
		}
	}

	/// <summary>Returns whether <paramref name="pageNumber"/> (one-based) exists.</summary>
	public virtual bool CanGoToPage(int pageNumber) => pageNumber >= 1 && pageNumber <= PageCount;

	/// <summary>Goes to the first page.</summary>
	public void FirstPage() => OnFirstPageCommand();

	/// <summary>Goes to the last page.</summary>
	public void LastPage() => OnLastPageCommand();

	/// <summary>Goes to the next page.</summary>
	public void NextPage() => OnNextPageCommand();

	/// <summary>Goes to the previous page.</summary>
	public void PreviousPage() => OnPreviousPageCommand();

	/// <summary>Goes to <paramref name="pageNumber"/> (one-based); an unavailable page is ignored.</summary>
	public void GoToPage(int pageNumber) => OnGoToPageCommand(pageNumber);

	/// <summary>Handles NavigationCommands.FirstPage.</summary>
	protected virtual void OnFirstPageCommand() => OnGoToPageCommand(1);

	/// <summary>Handles NavigationCommands.LastPage.</summary>
	protected virtual void OnLastPageCommand() => OnGoToPageCommand(PageCount);

	/// <summary>Handles NavigationCommands.NextPage.</summary>
	protected virtual void OnNextPageCommand() => OnGoToPageCommand(MasterPageNumber + 1);

	/// <summary>Handles NavigationCommands.PreviousPage.</summary>
	protected virtual void OnPreviousPageCommand() => OnGoToPageCommand(MasterPageNumber - 1);

	/// <summary>Handles NavigationCommands.GoToPage.</summary>
	protected virtual void OnGoToPageCommand(int pageNumber)
	{
		if (CanGoToPage(pageNumber)) SetMasterPage(pageNumber);
	}

	void SetMasterPage(int number)
	{
		bool changed = number != MasterPageNumber;
		SetValue(MasterPageNumberPropertyKey, number);
		UpdateCanGo();
		SyncViews();
		if (changed) OnMasterPageNumberChanged();
	}

	/// <summary>Called after the page count or the page state was refreshed.</summary>
	internal virtual void OnPagesRefreshed()
	{
	}

	void RefreshPages()
	{
		int count = _paginator?.PageCount ?? 0;
		SetValue(PageCountPropertyKey, count);
		int master = count == 0 ? 0 : Math.Max(1, Math.Min(MasterPageNumber, count));
		SetMasterPage(master);
		OnPagesRefreshed();
	}

	void UpdateCanGo()
	{
		SetValue(CanGoToPreviousPagePropertyKey, MasterPageNumber > 1);
		SetValue(CanGoToNextPagePropertyKey, MasterPageNumber < PageCount);
	}

	void SyncViews()
	{
		if (_views == null) return;
		foreach (var view in _views)
		{
			view.DocumentPaginator = _paginator;
			view.PageNumber = Math.Max(0, MasterPageNumber - 1);
		}
	}
}
