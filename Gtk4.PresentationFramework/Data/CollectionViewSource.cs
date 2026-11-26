using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Windows.Data;

/// <summary>
/// Provides a view of a collection for data binding, with support for sorting,
/// filtering and grouping. Mirrors <c>System.Windows.Data.CollectionViewSource</c>.
/// </summary>
public partial class CollectionViewSource : DependencyObject
{
	// Cache: source object → its default view.
	static readonly ConditionalWeakTable<object, CollectionView> _cache =
		new ConditionalWeakTable<object, CollectionView>();

	// ── Source ───────────────────────────────────────────────────────────────

	/// <summary>Identifies the <see cref="Source"/> dependency property.</summary>
	public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
		nameof(Source), typeof(object), typeof(CollectionViewSource), new PropertyMetadata(null, OnSourceChanged));

	static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((CollectionViewSource)d)._view = null; // invalidate

	/// <summary>
	/// Gets or sets the collection to view, or a <see cref="DataSourceProvider"/> whose <c>Data</c> is used.
	/// </summary>
	public object? Source
	{
		get => GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

	// ── View ─────────────────────────────────────────────────────────────────

	CollectionView? _view;

	/// <summary>Gets the <see cref="ICollectionView"/> over <see cref="Source"/>.</summary>
	public ICollectionView? View
	{
		get
		{
			var source = Source;
			var items = source is DataSourceProvider provider ? provider.Data as IEnumerable : source as IEnumerable;
			if (items == null) return null;
			if (_view == null)
			{
				_view = CreateView(items);
				// U6: forward instance SortDescriptions/GroupDescriptions to the view.
				foreach (var sd in SortDescriptions)
					_view.SortDescriptions.Add(sd);
				foreach (var gd in GroupDescriptions)
					_view.GroupDescriptions.Add(gd);
				if (SortDescriptions.Count > 0 || GroupDescriptions.Count > 0)
					_view.Refresh();
				InstallFilter(_view);
			}
			return _view;
		}
	}

	/// <summary>Sort descriptions applied to the view when it is created.</summary>
	public SortDescriptionCollection SortDescriptions { get; } = new SortDescriptionCollection();

	/// <summary>Group descriptions applied to the view when it is created.</summary>
	public ObservableCollection<GroupDescription> GroupDescriptions { get; } =
		new ObservableCollection<GroupDescription>();

	// WPF: an IList gets a ListCollectionView, any other sequence a plain CollectionView.
	static CollectionView CreateView(IEnumerable source) =>
		source is IList list ? new ListCollectionView(list) : new CollectionView(source);

	// A CollectionViewSource used as a binding source stands for its view (WPF binds to CollectionViewSource.View).
	// The source raises no change notification here, so a later Source change is seen on the next resolve only.
	internal static object? UnwrapView(object? source) =>
		source is CollectionViewSource cvs ? (object?)cvs.View ?? cvs : source;

	// ── Static helper ────────────────────────────────────────────────────────

	/// <summary>
	/// Returns the cached default view for <paramref name="source"/>: the source itself when it is an
	/// <see cref="ICollectionView"/>, else a stable view over the sequence (a string yields a view over its
	/// characters). Returns <see langword="null"/> for a null or non-enumerable source.
	/// </summary>
	public static ICollectionView? GetDefaultView(object? source)
	{
		if (source == null) return null;
		if (source is ICollectionView cv) return cv;
		if (source is not IEnumerable seq) return null;
		return _cache.GetValue(seq, s =>
		{
			var view = CreateView((IEnumerable)s);
			view.IsDefaultView = true;
			return view;
		});
	}
}