using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;

namespace System.Windows.Data;

public partial class CollectionView
{
	/// <inheritdoc/>
	public virtual IDisposable DeferRefresh()
	{
		_deferLevel++;
		return new DeferScope(this);
	}

	/// <inheritdoc/>
	public virtual void Refresh()
	{
		var oldItem = CurrentItem;
		int oldPosition = _currentPosition;
		int oldCount = _view.Count;
		Rebuild();

		// Keep the current item when it survives; otherwise stay at its position, clamped into the view.
		int position = oldItem != null ? _view.IndexOf(oldItem) : -1;
		if (position < 0) position = _view.Count == 0 ? -1 : Math.Min(Math.Max(oldPosition, 0), _view.Count - 1);
		bool itemChanged = !ReferenceEquals(oldItem, position >= 0 ? _view[position] : null);
		bool positionChanged = position != _currentPosition;
		_currentPosition = position;

		CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		if (oldCount != _view.Count)
		{
			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(nameof(IsEmpty));
		}
		if (itemChanged) RaiseCurrentChanged(positionChanged);
		else if (positionChanged) OnPropertyChanged(nameof(CurrentPosition));
	}

	void RefreshOrDefer()
	{
		if (_deferLevel == 0) Refresh();
	}

	void Rebuild()
	{
		_view.Clear();
		_viewSource.Clear();
		_groupObjects.Clear();

		// Null items pass through when there is no filter or the filter accepts them.
		var kept = new List<KeyValuePair<object?, int>>();
		int sourceIndex = 0;
		foreach (var item in _source)
		{
			if (_filter == null || _filter(item!)) kept.Add(new KeyValuePair<object?, int>(item, sourceIndex));
			sourceIndex++;
		}

		// Ties keep source order, so a view position maps to one source slot even for equal items.
		if (SortDescriptions.Count > 0)
			kept.Sort((a, b) =>
			{
				int c = CompareItems(a.Key, b.Key);
				return c != 0 ? c : a.Value.CompareTo(b.Value);
			});

		// WPF enumerates a grouped view group by group: items of one group are contiguous, groups keep the order
		// of their first item. Rendering group headers relies on that.
		if (GroupDescriptions.Count > 0) kept = OrderByGroup(kept);

		foreach (var entry in kept)
		{
			_view.Add(entry.Key);
			_viewSource.Add(entry.Value);
		}

		if (GroupDescriptions.Count > 0)
		{
			foreach (var g in BuildGroups()) _groupObjects.Add(g);
			Groups = new ReadOnlyObservableCollection<object>(_groupObjects);
		}
		else
		{
			Groups = null;
		}
	}

	int CompareItems(object? x, object? y)
	{
		foreach (var sd in SortDescriptions)
		{
			var xv = GetPropertyValue(x, sd.PropertyName);
			var yv = GetPropertyValue(y, sd.PropertyName);
			int c = Comparer<object?>.Default.Compare(xv, yv);
			if (c != 0) return sd.Direction == ListSortDirection.Ascending ? c : -c;
		}
		return 0;
	}

	static object? GetPropertyValue(object? obj, string? propertyName)
	{
		if (obj == null || string.IsNullOrEmpty(propertyName)) return obj;
		var prop = obj.GetType().GetProperty(propertyName!, BindingFlags.Public | BindingFlags.Instance);
		return prop?.GetValue(obj);
	}

	List<KeyValuePair<object?, int>> OrderByGroup(List<KeyValuePair<object?, int>> entries)
	{
		var gd = GroupDescriptions[0];
		var rank = new Dictionary<string, int>();
		var keyed = new List<(KeyValuePair<object?, int> Entry, int Rank)>();
		foreach (var entry in entries)
		{
			// A null item has no group key; it is placed after every group.
			if (entry.Key == null)
			{
				keyed.Add((entry, int.MaxValue));
				continue;
			}
			string key = gd.GroupNameFromItem(entry.Key, 0, _culture)?.ToString() ?? string.Empty;
			if (!rank.TryGetValue(key, out int r)) rank[key] = r = rank.Count;
			keyed.Add((entry, r));
		}
		var result = new List<KeyValuePair<object?, int>>(entries.Count);
		foreach (var item in keyed.Select((k, i) => (k, i)).OrderBy(t => t.k.Rank).ThenBy(t => t.i))
			result.Add(item.k.Entry);
		return result;
	}

	List<CollectionViewGroup> BuildGroups()
	{
		var gd = GroupDescriptions[0]; // single-level only
		var dict = new Dictionary<string, CollectionViewGroup>();
		var order = new List<string>();
		foreach (var item in _view)
		{
			if (item == null) continue; // no meaningful group key
			var rawKey = gd.GroupNameFromItem(item, 0, _culture);
			var key = rawKey?.ToString() ?? string.Empty;
			if (!dict.TryGetValue(key, out var grp))
			{
				grp = new CollectionViewGroup(rawKey);
				dict[key] = grp;
				order.Add(key);
			}
			grp.AddItem(item);
		}
		var result = new List<CollectionViewGroup>();
		foreach (var k in order) result.Add(dict[k]);
		return result;
	}

	sealed class DeferScope : IDisposable
	{
		CollectionView? _owner;

		internal DeferScope(CollectionView owner) => _owner = owner;

		public void Dispose()
		{
			var owner = _owner;
			if (owner == null) return;
			_owner = null;
			if (--owner._deferLevel == 0) owner.Refresh();
		}
	}
}
