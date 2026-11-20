using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace System.ComponentModel;

/// <summary>An observable collection of <see cref="SortDescription"/>.</summary>
public class SortDescriptionCollection : Collection<SortDescription>, INotifyCollectionChanged
{
	/// <summary>A shared empty collection that is never changed.</summary>
	public static readonly SortDescriptionCollection Empty = new SortDescriptionCollection();

	NotifyCollectionChangedEventHandler? _changed;

	event NotifyCollectionChangedEventHandler? INotifyCollectionChanged.CollectionChanged
	{
		add => _changed += value;
		remove => _changed -= value;
	}

	/// <inheritdoc/>
	protected override void InsertItem(int index, SortDescription item)
	{
		base.InsertItem(index, item);
		Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
	}

	/// <inheritdoc/>
	protected override void RemoveItem(int index)
	{
		var old = this[index];
		base.RemoveItem(index);
		Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, old, index));
	}

	/// <inheritdoc/>
	protected override void SetItem(int index, SortDescription item)
	{
		var old = this[index];
		base.SetItem(index, item);
		Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, old, index));
	}

	/// <inheritdoc/>
	protected override void ClearItems()
	{
		base.ClearItems();
		Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	void Raise(NotifyCollectionChangedEventArgs e) => _changed?.Invoke(this, e);
}
