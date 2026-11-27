using System.Collections;
using System.Collections.Specialized;

namespace System.Windows.Data;

/// <summary>
/// Holds a collection inside a <see cref="CompositeCollection"/>; the items of <see cref="Collection"/> take the
/// container's place in the composite.
/// </summary>
public class CollectionContainer : DependencyObject
{
	/// <summary>Identifies the <see cref="Collection"/> property.</summary>
	public static readonly DependencyProperty CollectionProperty =
		DependencyProperty.Register(nameof(Collection), typeof(IEnumerable), typeof(CollectionContainer),
			new PropertyMetadata(null, OnCollectionPropertyChanged));

	INotifyCollectionChanged? _watched;

	/// <summary>Raised when <see cref="Collection"/> is replaced or its content changes.</summary>
	internal event EventHandler? ContentChanged;

	/// <summary>Gets or sets the collection whose items the container contributes.</summary>
	public IEnumerable? Collection
	{
		get => (IEnumerable?)GetValue(CollectionProperty);
		set => SetValue(CollectionProperty, value);
	}

	static void OnCollectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> ((CollectionContainer)d).OnCollectionReplaced(e.NewValue as IEnumerable);

	void OnCollectionReplaced(IEnumerable? collection)
	{
		if (_watched != null) _watched.CollectionChanged -= OnWatchedChanged;
		_watched = collection as INotifyCollectionChanged;
		if (_watched != null) _watched.CollectionChanged += OnWatchedChanged;
		ContentChanged?.Invoke(this, EventArgs.Empty);
	}

	void OnWatchedChanged(object? sender, NotifyCollectionChangedEventArgs e)
		=> ContentChanged?.Invoke(this, EventArgs.Empty);

	internal int ItemCount
	{
		get
		{
			int n = 0;
			if (Collection != null) foreach (var _ in Collection) n++;
			return n;
		}
	}
}
