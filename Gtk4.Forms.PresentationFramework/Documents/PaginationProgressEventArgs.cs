namespace System.Windows.Documents;

/// <summary>Represents the method that handles <see cref="DynamicDocumentPaginator.PaginationProgress"/>.</summary>
public delegate void PaginationProgressEventHandler(object sender, PaginationProgressEventArgs e);

/// <summary>Data for <see cref="DynamicDocumentPaginator.PaginationProgress"/>.</summary>
public class PaginationProgressEventArgs : EventArgs
{
	/// <summary>Creates the data for <paramref name="count"/> pages from <paramref name="start"/>.</summary>
	/// <exception cref="ArgumentOutOfRangeException">start or count is negative.</exception>
	public PaginationProgressEventArgs(int start, int count)
	{
		if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
		if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
		Start = start;
		Count = count;
	}

	/// <summary>Gets the zero-based number of the first page that was paginated.</summary>
	public int Start { get; }

	/// <summary>Gets the number of pages that were paginated.</summary>
	public int Count { get; }
}
