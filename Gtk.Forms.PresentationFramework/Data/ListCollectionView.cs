using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace System.Windows.Data
{
	/// <summary>
	/// An <see cref="ICollectionView"/> over an <see cref="IList"/> or <see cref="IEnumerable"/>
	/// source, applying filter → sort → group to produce the visible sequence.
	/// Passes through <see cref="INotifyCollectionChanged"/> from the source.
	/// </summary>
	public class ListCollectionView : ICollectionView
	{
		readonly IEnumerable _source;
		readonly List<object?> _view = new List<object?>();
		readonly ObservableCollection<object> _groupObjects = new ObservableCollection<object>();

		int _currentPosition = -1;

		// ── ICollectionView ──────────────────────────────────────────────────────

		public IEnumerable SourceCollection => _source;

		public Predicate<object>? Filter { get; set; }

		public ObservableCollection<SortDescription> SortDescriptions { get; } =
			new ObservableCollection<SortDescription>();

		public ObservableCollection<GroupDescription> GroupDescriptions { get; } =
			new ObservableCollection<GroupDescription>();

		public ReadOnlyObservableCollection<object>? Groups { get; private set; }

		public object? CurrentItem =>
			_currentPosition >= 0 && _currentPosition < _view.Count
				? _view[_currentPosition] : null;

		public int CurrentPosition => _currentPosition;

		public event EventHandler? CurrentChanged;
		public event NotifyCollectionChangedEventHandler? CollectionChanged;

		// ── Construction ─────────────────────────────────────────────────────────

		public ListCollectionView(IEnumerable source)
		{
			_source = source ?? throw new ArgumentNullException(nameof(source));
			if (source is INotifyCollectionChanged incc)
				incc.CollectionChanged += OnSourceChanged;
			Refresh();
		}

		// ── Current navigation ───────────────────────────────────────────────────

		public bool MoveCurrentTo(object? item)
		{
			var idx = _view.IndexOf(item);
			return MoveCurrentToPosition(idx);
		}

		public bool MoveCurrentToPosition(int position)
		{
			if (position < -1 || position >= _view.Count) return false;
			if (_currentPosition == position) return true;
			_currentPosition = position;
			CurrentChanged?.Invoke(this, EventArgs.Empty);
			return _currentPosition >= 0 && _currentPosition < _view.Count;
		}

		public bool MoveCurrentToFirst() => MoveCurrentToPosition(_view.Count > 0 ? 0 : -1);
		public bool MoveCurrentToLast() => MoveCurrentToPosition(_view.Count - 1);
		public bool MoveCurrentToNext() => MoveCurrentToPosition(_currentPosition + 1);
		public bool MoveCurrentToPrevious() => MoveCurrentToPosition(_currentPosition - 1);

		// ── Refresh ──────────────────────────────────────────────────────────────

		public void Refresh()
		{
			_view.Clear();
			_groupObjects.Clear();

			// 1. Filter.
			// U5: null items are kept (WPF default view preserves nulls); Filter/Sort/Group
			// guard against null internally.
			foreach (var item in _source)
			{
				if (item == null)
				{
					// Null items pass through when there is no filter or the filter accepts them.
					if (Filter == null || Filter(null!))
						_view.Add(null);
					continue;
				}
				if (Filter != null && !Filter(item)) continue;
				_view.Add(item);
			}

			// 2. Sort.
			if (SortDescriptions.Count > 0)
				_view.Sort(CompareItems);

			// 3. Group.
			if (GroupDescriptions.Count > 0)
			{
				var groups = BuildGroups();
				foreach (var g in groups) _groupObjects.Add(g);
				Groups = new ReadOnlyObservableCollection<object>(_groupObjects);
			}
			else
			{
				Groups = null;
			}

			// U7c: clamp current position and raise CurrentChanged when CurrentItem changes.
			var prevCurrentItem = CurrentItem;
			if (_currentPosition >= _view.Count)
				_currentPosition = _view.Count - 1;
			if (!ReferenceEquals(prevCurrentItem, CurrentItem))
				CurrentChanged?.Invoke(this, EventArgs.Empty);

			CollectionChanged?.Invoke(this,
				new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		// ── IEnumerable ──────────────────────────────────────────────────────────

		public IEnumerator GetEnumerator() => _view.GetEnumerator();

		// ── Privates ─────────────────────────────────────────────────────────────

		void OnSourceChanged(object? sender, NotifyCollectionChangedEventArgs e) => Refresh();

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
			var prop = obj.GetType().GetProperty(propertyName!,
				BindingFlags.Public | BindingFlags.Instance);
			return prop?.GetValue(obj);
		}

		List<CollectionViewGroup> BuildGroups()
		{
			var gd = GroupDescriptions[0]; // single-level only
			// Use string key to avoid nullable constraint on Dictionary key.
			var dict = new Dictionary<string, CollectionViewGroup>();
			var order = new List<string>();
			foreach (var item in _view)
			{
				// U5: null items are excluded from grouping (no meaningful group key).
				if (item == null) continue;
				var rawKey = gd.GroupNameFromItem(item, 0, CultureInfo.CurrentCulture);
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
	}
}
