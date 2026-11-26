using System.Collections;

namespace System.Windows.Data;

/// <summary>The default <see cref="CollectionView"/> for an <see cref="IList"/> source.</summary>
public partial class ListCollectionView : CollectionView, System.ComponentModel.IEditableCollectionView
{
	/// <summary>Creates a view over <paramref name="list"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="list"/> is <see langword="null"/>.</exception>
	public ListCollectionView(IList list) : base(list) { }

	/// <summary>Gets the source list.</summary>
	public IList InternalList => (IList)SourceCollection;
}
