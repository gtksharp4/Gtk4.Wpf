using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Data;

/// <summary>
/// A view over a collection: filter, sort and group, plus a current item. Rebuilt on every source
/// change, so the whole view is re-evaluated rather than patched.
/// </summary>
public partial class CollectionView : ICollectionView, INotifyPropertyChanged
{
	/// <summary>True for the view <see cref="CollectionViewSource.GetDefaultView"/> makes for a plain source.</summary>
	internal bool IsDefaultView { get; set; }

	readonly IEnumerable _source;
	readonly List<object?> _view = new List<object?>();
	readonly List<int> _viewSource = new List<int>();
	readonly ObservableCollection<object> _groupObjects = new ObservableCollection<object>();
	Predicate<object>? _filter;
	CultureInfo _culture = CultureInfo.CurrentCulture;
	int _deferLevel;

	/// <summary>Creates a view over <paramref name="collection"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
	public CollectionView(IEnumerable collection)
	{
		_source = collection ?? throw new ArgumentNullException(nameof(collection));
		SortDescriptions = new SortDescriptionCollection();
		((INotifyCollectionChanged)SortDescriptions).CollectionChanged += (_, _) => RefreshOrDefer();
		GroupDescriptions.CollectionChanged += (_, _) => RefreshOrDefer();
		if (collection is INotifyCollectionChanged incc)
			incc.CollectionChanged += (_, _) => RefreshOrDefer();
		Rebuild();
		_currentPosition = _view.Count > 0 ? 0 : -1;
	}

	/// <inheritdoc/>
	public virtual CultureInfo Culture
	{
		get => _culture;
		set
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (Equals(_culture, value)) return;
			_culture = value;
			RefreshOrDefer();
		}
	}

	/// <inheritdoc/>
	public virtual IEnumerable SourceCollection => _source;

	/// <inheritdoc/>
	public virtual Predicate<object>? Filter
	{
		get => _filter;
		set
		{
			_filter = value;
			RefreshOrDefer();
		}
	}

	/// <inheritdoc/>
	public virtual bool CanFilter => true;

	/// <inheritdoc/>
	public virtual SortDescriptionCollection SortDescriptions { get; }

	/// <inheritdoc/>
	public virtual bool CanSort => true;

	/// <inheritdoc/>
	public virtual ObservableCollection<GroupDescription> GroupDescriptions { get; } =
		new ObservableCollection<GroupDescription>();

	/// <inheritdoc/>
	public virtual bool CanGroup => true;

	/// <inheritdoc/>
	public virtual ReadOnlyObservableCollection<object>? Groups { get; private set; }

	/// <summary>Gets the number of items in the view.</summary>
	public virtual int Count => _view.Count;

	/// <inheritdoc/>
	public virtual bool IsEmpty => _view.Count == 0;

	/// <inheritdoc/>
	public virtual bool Contains(object item) => _view.Contains(item);

	/// <summary>Returns the position of <paramref name="item"/> in the view, or -1.</summary>
	public virtual int IndexOf(object? item) => _view.IndexOf(item);

	/// <summary>Returns the item at <paramref name="index"/> in the view.</summary>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the view.</exception>
	public virtual object? GetItemAt(int index)
	{
		if (index < 0 || index >= _view.Count) throw new ArgumentOutOfRangeException(nameof(index));
		return _view[index];
	}

	/// <inheritdoc/>
	public virtual IEnumerator GetEnumerator() => _view.GetEnumerator();

	/// <inheritdoc/>
	public virtual event NotifyCollectionChangedEventHandler? CollectionChanged;

	/// <summary>Raised when a view property such as <c>Count</c> or <c>CurrentItem</c> changes.</summary>
	protected virtual event PropertyChangedEventHandler? PropertyChanged;

	event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
	{
		add => PropertyChanged += value;
		remove => PropertyChanged -= value;
	}

	/// <summary>Raises <see cref="PropertyChanged"/>.</summary>
	protected void OnPropertyChanged(string propertyName) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
