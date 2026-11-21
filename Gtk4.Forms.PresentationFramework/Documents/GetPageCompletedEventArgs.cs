using System.ComponentModel;

namespace System.Windows.Documents;

/// <summary>Represents the method that handles <see cref="DocumentPaginator.GetPageCompleted"/>.</summary>
public delegate void GetPageCompletedEventHandler(object sender, GetPageCompletedEventArgs e);

/// <summary>Data for <see cref="DocumentPaginator.GetPageCompleted"/>.</summary>
public class GetPageCompletedEventArgs : AsyncCompletedEventArgs
{
	readonly DocumentPage? _page;

	/// <summary>Creates the data for a finished asynchronous GetPage.</summary>
	public GetPageCompletedEventArgs(DocumentPage? page, int pageNumber, Exception? error, bool cancelled,
		object? userState) : base(error, cancelled, userState)
	{
		_page = page;
		PageNumber = pageNumber;
	}

	/// <summary>Gets the page; throws when the operation failed or was cancelled, like WPF.</summary>
	public DocumentPage? DocumentPage
	{
		get
		{
			RaiseExceptionIfNecessary();
			return _page;
		}
	}

	/// <summary>Gets the zero-based number of the requested page.</summary>
	public int PageNumber { get; }
}
