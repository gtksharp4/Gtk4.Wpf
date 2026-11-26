using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace System.Windows.Annotations;

/// <summary>An object that may sit in only one collection at a time.</summary>
interface IOwned
{
	bool Owned { get; set; }
}

/// <summary>
/// A collection that rejects null and items already owned by another collection (an
/// <see cref="InvalidOperationException"/>, as in WPF) and reports an item's own change.
/// </summary>
sealed class OwnedCollection<T> : ObservableCollection<T> where T : class, IOwned
{
	internal event PropertyChangedEventHandler? ItemChanged;

	protected override void InsertItem(int index, T item)
	{
		Claim(item);
		base.InsertItem(index, item);
		Watch(item);
	}

	protected override void SetItem(int index, T item)
	{
		var old = this[index];
		Claim(item);
		base.SetItem(index, item);
		Release(old);
		Watch(item);
	}

	protected override void RemoveItem(int index)
	{
		var old = this[index];
		base.RemoveItem(index);
		Release(old);
	}

	protected override void ClearItems()
	{
		var old = new List<T>(this);
		base.ClearItems();
		foreach (var item in old) Release(item);
	}

	static void Claim(T item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		if (item.Owned) throw new InvalidOperationException("The object already belongs to a collection.");
		item.Owned = true;
	}

	void Watch(T item)
	{
		if (item is INotifyPropertyChanged n) n.PropertyChanged += OnItemChanged;
	}

	void Release(T item)
	{
		item.Owned = false;
		if (item is INotifyPropertyChanged n) n.PropertyChanged -= OnItemChanged;
	}

	void OnItemChanged(object? sender, PropertyChangedEventArgs e) => ItemChanged?.Invoke(sender, e);
}

/// <summary>A collection that rejects null items.</summary>
sealed class NonNullCollection<T> : ObservableCollection<T> where T : class
{
	protected override void InsertItem(int index, T item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		base.InsertItem(index, item);
	}

	protected override void SetItem(int index, T item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		base.SetItem(index, item);
	}
}
