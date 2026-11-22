using System.ComponentModel;

namespace System.Windows.Documents;

/// <summary>A paginator that can paginate in the background and map positions to pages.</summary>
public abstract class DynamicDocumentPaginator : DocumentPaginator
{
	/// <summary>Gets the zero-based number of the page holding <paramref name="contentPosition"/>.</summary>
	public abstract int GetPageNumber(ContentPosition contentPosition);

	/// <summary>Gets the position at the start of <paramref name="page"/>.</summary>
	public abstract ContentPosition GetPagePosition(DocumentPage page);

	/// <summary>Gets the position of <paramref name="element"/> in the content.</summary>
	public abstract ContentPosition GetObjectPosition(object element);

	/// <summary>Gets or sets whether pagination continues in the background.</summary>
	public virtual bool IsBackgroundPaginationEnabled
	{
		get => false;
		set { }
	}

	/// <summary>Occurs when pagination has finished.</summary>
	public event EventHandler? PaginationCompleted;

	/// <summary>Occurs while pagination advances.</summary>
	public event PaginationProgressEventHandler? PaginationProgress;

	/// <summary>Occurs when <see cref="GetPageNumberAsync(ContentPosition, object)"/> finishes.</summary>
	public event GetPageNumberCompletedEventHandler? GetPageNumberCompleted;

	/// <summary>Raises <see cref="PaginationCompleted"/>.</summary>
	protected virtual void OnPaginationCompleted(EventArgs e) => PaginationCompleted?.Invoke(this, e);

	/// <summary>Raises <see cref="PaginationProgress"/>.</summary>
	protected virtual void OnPaginationProgress(PaginationProgressEventArgs e) => PaginationProgress?.Invoke(this, e);

	/// <summary>Raises <see cref="GetPageNumberCompleted"/>.</summary>
	protected virtual void OnGetPageNumberCompleted(GetPageNumberCompletedEventArgs e) =>
		GetPageNumberCompleted?.Invoke(this, e);

	/// <summary>Looks a position up without blocking; the state is this paginator.</summary>
	public virtual void GetPageNumberAsync(ContentPosition contentPosition) =>
		GetPageNumberAsync(contentPosition, this);

	/// <summary>Looks a position up without blocking; completion is raised from the dispatcher.</summary>
	/// <exception cref="ArgumentNullException">An argument is null.</exception>
	public virtual void GetPageNumberAsync(ContentPosition contentPosition, object userState)
	{
		if (contentPosition == null) throw new ArgumentNullException(nameof(contentPosition));
		if (userState == null) throw new ArgumentNullException(nameof(userState));
		Post(userState, cancelled =>
		{
			int number = 0;
			Exception? error = null;
			if (!cancelled)
			{
				try { number = GetPageNumber(contentPosition); }
				catch (Exception ex) { error = ex; }
			}
			OnGetPageNumberCompleted(new GetPageNumberCompletedEventArgs(contentPosition, number, error, cancelled,
				userState));
		});
	}
}
