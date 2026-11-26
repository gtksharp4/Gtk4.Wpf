using System.Collections.ObjectModel;
using System.ComponentModel;

namespace System.Windows.Data;

/// <summary>
/// Represents a group created by a <see cref="GroupDescription"/> in a <see cref="ICollectionView"/>.
/// </summary>
public class CollectionViewGroup
{
	readonly ObservableCollection<object> _items = new ObservableCollection<object>();

	/// <summary>The common group key.</summary>
	public object? Name { get; }

	/// <summary>The items in this group.</summary>
	public ReadOnlyObservableCollection<object> Items { get; }

	/// <summary>Number of items in this group.</summary>
	public int ItemCount => _items.Count;

	internal CollectionViewGroup(object? name)
	{
		Name = name;
		Items = new ReadOnlyObservableCollection<object>(_items);
	}

	internal void AddItem(object item) => _items.Add(item);
}