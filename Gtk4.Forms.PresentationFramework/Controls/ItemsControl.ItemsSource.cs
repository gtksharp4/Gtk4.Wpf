using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;

namespace System.Windows.Controls;

public partial class ItemsControl
{
	/// <summary>The collection of items to display.</summary>
	public static readonly DependencyProperty ItemsSourceProperty =
		DependencyProperty.Register(
			nameof(ItemsSource),
			typeof(IEnumerable),
			typeof(ItemsControl),
			new PropertyMetadata(null, OnItemsSourceChanged));

	static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ItemsControl)d).OnItemsSourceChanged(
			e.OldValue as IEnumerable,
			e.NewValue as IEnumerable);
	}

	/// <summary>Gets or sets the items source.</summary>
	public IEnumerable? ItemsSource
	{
		get => (IEnumerable?)GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}

	INotifyCollectionChanged? _incc;
	ICollectionView? _collectionView;

	void OnItemsSourceChanged(IEnumerable? oldSource, IEnumerable? newSource)
	{
		if (newSource != null && !_itemsFromSource && Items.Count > 0)
			throw new InvalidOperationException("Items collection must be empty before using ItemsSource.");
		if (_incc != null)
		{
			_incc.CollectionChanged -= OnCollectionChanged;
			_incc = null;
		}
		_collectionView = null;

		if (newSource != null)
		{
			// Wrap the source in a default CollectionView (honours filter/sort/group).
			_collectionView = CollectionViewSource.GetDefaultView(newSource);
			_incc = _collectionView;
			if (_incc != null) _incc.CollectionChanged += OnCollectionChanged;
		}

		RebuildItems();
		OnCollectionViewChanged(_collectionView);
	}

	/// <summary>Called after ItemsSource changed, with its default view (or null).</summary>
	internal virtual void OnCollectionViewChanged(ICollectionView? view) { }

	void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		RebuildItems();
	}

	/// <summary>
	/// Returns the effective item sequence: the <see cref="ICollectionView"/> when available,
	/// otherwise the raw <see cref="ItemsSource"/>.
	/// </summary>
	IEnumerable EffectiveItems => (IEnumerable?)_collectionView ?? ItemsSource ?? Array.Empty<object>();
}
