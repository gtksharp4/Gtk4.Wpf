using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Data;

namespace System.ComponentModel;

/// <summary>
/// Enables collections to have the functionalities of current-record management,
/// custom sorting, filtering, and grouping.
/// </summary>
public interface ICollectionView : IEnumerable, INotifyCollectionChanged
{
	/// <summary>Gets or sets the culture used for comparisons and group names.</summary>
	CultureInfo Culture { get; set; }

	/// <summary>Returns whether the view contains <paramref name="item"/>.</summary>
	bool Contains(object item);

	/// <summary>Returns the underlying (un-filtered) source collection.</summary>
	IEnumerable SourceCollection { get; }

	/// <summary>Gets or sets a callback to filter items.</summary>
	Predicate<object>? Filter { get; set; }

	/// <summary>Gets a value that indicates whether the view supports filtering.</summary>
	bool CanFilter { get; }

	/// <summary>Gets the sort descriptions.</summary>
	SortDescriptionCollection SortDescriptions { get; }

	/// <summary>Gets a value that indicates whether the view supports sorting.</summary>
	bool CanSort { get; }

	/// <summary>Gets the group descriptions.</summary>
	ObservableCollection<GroupDescription> GroupDescriptions { get; }

	/// <summary>Gets a value that indicates whether the view supports grouping.</summary>
	bool CanGroup { get; }

	/// <summary>Gets the top-level groups, or <c>null</c> when grouping is not active.</summary>
	ReadOnlyObservableCollection<object>? Groups { get; }

	/// <summary>Gets a value that indicates whether the view is empty.</summary>
	bool IsEmpty { get; }

	/// <summary>Gets the current item in the view.</summary>
	object? CurrentItem { get; }

	/// <summary>Gets the ordinal position of <see cref="CurrentItem"/>; -1 before the first item.</summary>
	int CurrentPosition { get; }

	/// <summary>Gets whether the current item is past the last item.</summary>
	bool IsCurrentAfterLast { get; }

	/// <summary>Gets whether the current item is before the first item.</summary>
	bool IsCurrentBeforeFirst { get; }

	/// <summary>Raised before <see cref="CurrentItem"/> changes; a handler may cancel the change.</summary>
	event CurrentChangingEventHandler CurrentChanging;

	/// <summary>Raised after <see cref="CurrentItem"/> changed.</summary>
	event EventHandler? CurrentChanged;

	/// <summary>Sets <see cref="CurrentItem"/> to the specified item.</summary>
	bool MoveCurrentTo(object? item);

	/// <summary>Sets <see cref="CurrentItem"/> to the item at the given position.</summary>
	bool MoveCurrentToPosition(int position);

	/// <summary>Moves to the first item.</summary>
	bool MoveCurrentToFirst();

	/// <summary>Moves to the last item.</summary>
	bool MoveCurrentToLast();

	/// <summary>Moves to the next item.</summary>
	bool MoveCurrentToNext();

	/// <summary>Moves to the previous item.</summary>
	bool MoveCurrentToPrevious();

	/// <summary>Defers <see cref="Refresh"/> until the returned object is disposed.</summary>
	IDisposable DeferRefresh();

	/// <summary>Re-applies filter, sort and grouping.</summary>
	void Refresh();
}
