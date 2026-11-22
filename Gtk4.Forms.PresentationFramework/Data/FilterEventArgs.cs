namespace System.Windows.Data;

/// <summary>Data for the <see cref="CollectionViewSource.Filter"/> event.</summary>
public class FilterEventArgs : EventArgs
{
	internal FilterEventArgs(object item)
	{
		Item = item;
		Accepted = true;
	}

	/// <summary>Gets the item to accept or reject.</summary>
	public object Item { get; }

	/// <summary>Gets or sets whether the item passes the filter (default true).</summary>
	public bool Accepted { get; set; }
}

/// <summary>Represents the method that handles <see cref="CollectionViewSource.Filter"/>.</summary>
public delegate void FilterEventHandler(object sender, FilterEventArgs e);
