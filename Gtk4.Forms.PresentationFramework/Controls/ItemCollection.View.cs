using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace System.Windows.Controls;

public sealed partial class ItemCollection
{
	// In ItemsSource mode the view members forward to the source's view, so currency, filter, sort and
	// grouping are shared with it. Inline mode uses the CollectionView over the inline list.

	/// <summary>Switches between inline mode (<see langword="null"/>) and the view of the ItemsSource.</summary>
	internal void SetSourceView(ICollectionView? view)
	{
		if (ReferenceEquals(view, _sourceView)) return;
		DetachSource();
		_sourceView = view;
		AttachSource();
	}

	IEnumerable<object?> SourceItems => _sourceView!.Cast<object?>();

	/// <inheritdoc/>
	public override CultureInfo Culture
	{
		get => _sourceView?.Culture ?? base.Culture;
		set { if (_sourceView != null) _sourceView.Culture = value; else base.Culture = value; }
	}

	/// <inheritdoc/>
	public override IEnumerable SourceCollection => _sourceView?.SourceCollection ?? base.SourceCollection;

	/// <inheritdoc/>
	public override Predicate<object>? Filter
	{
		get => _sourceView != null ? _sourceView.Filter : base.Filter;
		set { if (_sourceView != null) _sourceView.Filter = value; else base.Filter = value; }
	}

	/// <inheritdoc/>
	public override bool CanFilter => _sourceView?.CanFilter ?? base.CanFilter;

	/// <inheritdoc/>
	public override SortDescriptionCollection SortDescriptions =>
		_sourceView?.SortDescriptions ?? base.SortDescriptions;

	/// <inheritdoc/>
	public override bool CanSort => _sourceView?.CanSort ?? base.CanSort;

	/// <inheritdoc/>
	public override ObservableCollection<GroupDescription> GroupDescriptions =>
		_sourceView?.GroupDescriptions ?? base.GroupDescriptions;

	/// <inheritdoc/>
	public override bool CanGroup => _sourceView?.CanGroup ?? base.CanGroup;

	/// <inheritdoc/>
	public override ReadOnlyObservableCollection<object>? Groups =>
		_sourceView != null ? _sourceView.Groups : base.Groups;

	/// <inheritdoc/>
	public override int Count =>
		_sourceView == null ? base.Count : _sourceView is CollectionView cv ? cv.Count : SourceItems.Count();

	/// <inheritdoc/>
	public override bool IsEmpty => _sourceView?.IsEmpty ?? base.IsEmpty;

	/// <inheritdoc/>
	public override bool Contains(object item) => _sourceView?.Contains(item) ?? base.Contains(item);

	/// <inheritdoc/>
	public override int IndexOf(object? item) =>
		_sourceView == null ? base.IndexOf(item)
		: _sourceView is CollectionView cv ? cv.IndexOf(item)
		: SourceItems.ToList().IndexOf(item);

	/// <inheritdoc/>
	public override object? GetItemAt(int index) =>
		_sourceView == null ? base.GetItemAt(index)
		: _sourceView is CollectionView cv ? cv.GetItemAt(index)
		: SourceItems.ElementAtOrDefault(index);

	/// <inheritdoc/>
	public override IEnumerator GetEnumerator() => _sourceView?.GetEnumerator() ?? base.GetEnumerator();

	/// <inheritdoc/>
	public override IDisposable DeferRefresh() => _sourceView?.DeferRefresh() ?? base.DeferRefresh();

	/// <inheritdoc/>
	public override void Refresh()
	{
		if (_sourceView != null) _sourceView.Refresh();
		else base.Refresh();
	}

	/// <inheritdoc/>
	public override object? CurrentItem => _sourceView != null ? _sourceView.CurrentItem : base.CurrentItem;

	/// <inheritdoc/>
	public override int CurrentPosition => _sourceView?.CurrentPosition ?? base.CurrentPosition;

	/// <inheritdoc/>
	public override bool IsCurrentAfterLast => _sourceView?.IsCurrentAfterLast ?? base.IsCurrentAfterLast;

	/// <inheritdoc/>
	public override bool IsCurrentBeforeFirst => _sourceView?.IsCurrentBeforeFirst ?? base.IsCurrentBeforeFirst;

	/// <inheritdoc/>
	public override bool MoveCurrentTo(object? item) => _sourceView?.MoveCurrentTo(item) ?? base.MoveCurrentTo(item);

	/// <inheritdoc/>
	public override bool MoveCurrentToPosition(int position) =>
		_sourceView?.MoveCurrentToPosition(position) ?? base.MoveCurrentToPosition(position);

	/// <inheritdoc/>
	public override bool MoveCurrentToFirst() => _sourceView?.MoveCurrentToFirst() ?? base.MoveCurrentToFirst();

	/// <inheritdoc/>
	public override bool MoveCurrentToLast() => _sourceView?.MoveCurrentToLast() ?? base.MoveCurrentToLast();

	/// <inheritdoc/>
	public override bool MoveCurrentToNext() => _sourceView?.MoveCurrentToNext() ?? base.MoveCurrentToNext();

	/// <inheritdoc/>
	public override bool MoveCurrentToPrevious() =>
		_sourceView?.MoveCurrentToPrevious() ?? base.MoveCurrentToPrevious();
}
