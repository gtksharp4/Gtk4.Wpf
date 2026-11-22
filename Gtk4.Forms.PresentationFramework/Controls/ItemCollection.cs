using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;

namespace System.Windows.Controls;

/// <summary>
/// The items of an <see cref="ItemsControl"/>. Holds the inline items, or reflects the view of
/// <see cref="ItemsControl.ItemsSource"/> (and then rejects every edit).
/// </summary>
public sealed partial class ItemCollection : CollectionView, IList
{
	readonly ItemsControl _owner;
	readonly ObservableCollection<object?> _inline;
	ICollectionView? _sourceView;
	bool _mutating;

	internal ItemCollection(ItemsControl owner) : this(owner, new ObservableCollection<object?>()) { }

	ItemCollection(ItemsControl owner, ObservableCollection<object?> inline) : base(inline)
	{
		_owner = owner;
		_inline = inline;
		base.CollectionChanged += OnInlineViewChanged;
		base.CurrentChanging += (s, e) => { if (_sourceView == null) _currentChanging?.Invoke(this, e); };
		base.CurrentChanged += (s, e) => { if (_sourceView == null) _currentChanged?.Invoke(this, e); };
		base.PropertyChanged += (s, e) => { if (_sourceView == null) _propertyChanged?.Invoke(this, e); };
	}

	internal bool IsUsingItemsSource => _sourceView != null;

	internal int InlineCount => _inline.Count;

	void Guard()
	{
		if (_sourceView != null)
			throw new InvalidOperationException(
				"Operation is not valid while ItemsSource is in use. "
				+ "Access and modify elements with ItemsControl.ItemsSource instead.");
	}

	void Mutate(Action edit)
	{
		Guard();
		_mutating = true;
		try { edit(); }
		finally { _mutating = false; }
		_owner.OnInlineItemsEdited();
	}

	void OnInlineViewChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (_sourceView != null) return;
		_changed?.Invoke(this, e);
		if (!_mutating) _owner.RebuildItems();
	}

	/// <summary>Adds an inline item and returns its index.</summary>
	/// <exception cref="InvalidOperationException">ItemsSource is in use.</exception>
	public int Add(object? newItem)
	{
		Mutate(() => _inline.Add(newItem));
		return _inline.Count - 1;
	}

	/// <summary>Inserts an inline item at view position <paramref name="index"/>.</summary>
	public void Insert(int index, object? item)
	{
		Guard();
		if (index < 0 || index > Count) throw new ArgumentOutOfRangeException(nameof(index));
		int at = Filter == null && SortDescriptions.Count == 0 ? index
			: index == Count ? _inline.Count
			: SourceIndexAt(index);
		Mutate(() => _inline.Insert(at, item));
	}

	/// <summary>Removes the first inline occurrence of <paramref name="item"/>.</summary>
	public void Remove(object? item) => Mutate(() => _inline.Remove(item));

	/// <summary>Removes the inline item at <paramref name="index"/>.</summary>
	public void RemoveAt(int index)
	{
		int at = InlineIndexOfViewItem(index);
		Mutate(() => _inline.RemoveAt(at));
	}

	int InlineIndexOfViewItem(int index)
	{
		Guard();
		if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
		return Filter == null && SortDescriptions.Count == 0 ? index : SourceIndexAt(index);
	}

	/// <summary>Removes every inline item.</summary>
	public void Clear() => Mutate(() => _inline.Clear());

	/// <summary>Gets the item at <paramref name="index"/> of the view; sets the inline item there.</summary>
	public object? this[int index]
	{
		get => GetItemAt(index);
		set
		{
			int at = InlineIndexOfViewItem(index);
			Mutate(() => _inline[at] = value);
		}
	}

	bool IList.Contains(object? value) => IndexOf(value) >= 0;

	bool IList.IsReadOnly => IsUsingItemsSource;

	bool IList.IsFixedSize => IsUsingItemsSource;

	int IList.Add(object? value) => Add(value);

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => ((ICollection)_inline).SyncRoot;

	void ICollection.CopyTo(Array array, int index)
	{
		foreach (var item in this) array.SetValue(item, index++);
	}
}
