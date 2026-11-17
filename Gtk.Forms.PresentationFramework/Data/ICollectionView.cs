using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace System.Windows.Data;

/// <summary>
/// Enables collections to have the functionalities of current-record management,
/// custom sorting, filtering, and grouping.
/// Mirrors <c>System.ComponentModel.ICollectionView</c>.
/// </summary>
public interface ICollectionView : IEnumerable, INotifyCollectionChanged
{
	/// <summary>Returns the underlying (un-filtered) source collection.</summary>
	IEnumerable SourceCollection { get; }

	/// <summary>Gets or sets a callback to filter items.</summary>
	Predicate<object>? Filter { get; set; }

	/// <summary>Gets a collection of <see cref="SortDescription"/>s.</summary>
	ObservableCollection<SortDescription> SortDescriptions { get; }

	/// <summary>Gets a collection of group descriptions.</summary>
	ObservableCollection<GroupDescription> GroupDescriptions { get; }

	/// <summary>Gets the top-level groups, or <c>null</c> when grouping is not active.</summary>
	ReadOnlyObservableCollection<object>? Groups { get; }

	/// <summary>Gets the current item in the view.</summary>
	object? CurrentItem { get; }

	/// <summary>Gets the ordinal position of <see cref="CurrentItem"/>.</summary>
	int CurrentPosition { get; }

	/// <summary>Raised when <see cref="CurrentItem"/> changes.</summary>
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

	/// <summary>Re-applies filter, sort and grouping.</summary>
	void Refresh();
}