namespace System.Windows.Data;

public partial class CollectionView
{
	/// <summary>The index in the source collection of the item at view position <paramref name="index"/>.</summary>
	internal int SourceIndexAt(int index) => _viewSource[index];
}
