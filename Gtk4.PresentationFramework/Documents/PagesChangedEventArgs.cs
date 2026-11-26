namespace System.Windows.Documents;

/// <summary>Represents the method that handles <see cref="DocumentPaginator.PagesChanged"/>.</summary>
public delegate void PagesChangedEventHandler(object sender, PagesChangedEventArgs e);

/// <summary>Data for <see cref="DocumentPaginator.PagesChanged"/>.</summary>
public class PagesChangedEventArgs : EventArgs
{
	/// <summary>Creates the data for <paramref name="count"/> pages from <paramref name="start"/>.</summary>
	/// <exception cref="ArgumentOutOfRangeException">start or count is negative.</exception>
	public PagesChangedEventArgs(int start, int count)
	{
		if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
		if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
		Start = start;
		Count = count;
	}

	/// <summary>Gets the zero-based number of the first changed page.</summary>
	public int Start { get; }

	/// <summary>Gets the number of changed pages.</summary>
	public int Count { get; }
}
