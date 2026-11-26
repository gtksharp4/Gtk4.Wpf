using System.ComponentModel;

namespace System.Windows.Documents;

/// <summary>Represents the method that handles <see cref="DynamicDocumentPaginator.GetPageNumberCompleted"/>.</summary>
public delegate void GetPageNumberCompletedEventHandler(object sender, GetPageNumberCompletedEventArgs e);

/// <summary>Data for <see cref="DynamicDocumentPaginator.GetPageNumberCompleted"/>.</summary>
public class GetPageNumberCompletedEventArgs : AsyncCompletedEventArgs
{
	readonly ContentPosition? _contentPosition;
	readonly int _pageNumber;

	/// <summary>Creates the data for a finished asynchronous GetPageNumber.</summary>
	public GetPageNumberCompletedEventArgs(ContentPosition? contentPosition, int pageNumber, Exception? error,
		bool cancelled, object? userState) : base(error, cancelled, userState)
	{
		_contentPosition = contentPosition;
		_pageNumber = pageNumber;
	}

	/// <summary>Gets the position that was looked up; throws when the operation failed or was cancelled.</summary>
	public ContentPosition? ContentPosition
	{
		get
		{
			RaiseExceptionIfNecessary();
			return _contentPosition;
		}
	}

	/// <summary>Gets the zero-based page number found; throws when the operation failed or was cancelled.</summary>
	public int PageNumber
	{
		get
		{
			RaiseExceptionIfNecessary();
			return _pageNumber;
		}
	}
}
