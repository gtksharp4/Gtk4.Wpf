using System.ComponentModel;

namespace System.Windows.Documents;

/// <summary>Lays content out in pages. The async members complete through the dispatcher.</summary>
public abstract partial class DocumentPaginator
{
	/// <summary>Gets the page at the zero-based <paramref name="pageNumber"/>.</summary>
	public abstract DocumentPage GetPage(int pageNumber);

	/// <summary>Gets whether <see cref="PageCount"/> is final.</summary>
	public abstract bool IsPageCountValid { get; }

	/// <summary>Gets the number of pages, computing it if needed.</summary>
	public abstract int PageCount { get; }

	/// <summary>Gets or sets the suggested page size.</summary>
	public abstract Size PageSize { get; set; }

	/// <summary>Gets the content being paginated.</summary>
	public abstract IDocumentPaginatorSource Source { get; }

	/// <summary>Occurs when <see cref="GetPageAsync(int, object)"/> finishes.</summary>
	public event GetPageCompletedEventHandler? GetPageCompleted;

	/// <summary>Occurs when <see cref="ComputePageCountAsync(object)"/> finishes.</summary>
	public event AsyncCompletedEventHandler? ComputePageCountCompleted;

	/// <summary>Occurs when pages change.</summary>
	public event PagesChangedEventHandler? PagesChanged;

	/// <summary>Raises <see cref="GetPageCompleted"/>.</summary>
	protected virtual void OnGetPageCompleted(GetPageCompletedEventArgs e) => GetPageCompleted?.Invoke(this, e);

	/// <summary>Raises <see cref="ComputePageCountCompleted"/>.</summary>
	protected virtual void OnComputePageCountCompleted(AsyncCompletedEventArgs e) =>
		ComputePageCountCompleted?.Invoke(this, e);

	/// <summary>Raises <see cref="PagesChanged"/>.</summary>
	protected virtual void OnPagesChanged(PagesChangedEventArgs e) => PagesChanged?.Invoke(this, e);

	/// <summary>Computes the page count so that <see cref="IsPageCountValid"/> becomes true.</summary>
	public virtual void ComputePageCount()
	{
		_ = PageCount;
	}
}
