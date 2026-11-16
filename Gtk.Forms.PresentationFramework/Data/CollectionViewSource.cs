using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Windows.Data
{
	/// <summary>
	/// Provides a view of a collection for data binding, with support for sorting,
	/// filtering and grouping. Mirrors <c>System.Windows.Data.CollectionViewSource</c>.
	/// </summary>
	public class CollectionViewSource
	{
		// Cache: source object → its default ListCollectionView.
		static readonly ConditionalWeakTable<object, ListCollectionView> _cache =
			new ConditionalWeakTable<object, ListCollectionView>();

		// ── Source ───────────────────────────────────────────────────────────────

		IEnumerable? _source;

		/// <summary>Gets or sets the underlying source collection.</summary>
		public IEnumerable? Source
		{
			get => _source;
			set
			{
				_source = value;
				_view = null; // invalidate
			}
		}

		// ── View ─────────────────────────────────────────────────────────────────

		ListCollectionView? _view;

		/// <summary>Gets the <see cref="ICollectionView"/> over <see cref="Source"/>.</summary>
		public ICollectionView? View
		{
			get
			{
				if (_source == null) return null;
				if (_view == null)
				{
					_view = new ListCollectionView(_source);
					// U6: forward instance SortDescriptions/GroupDescriptions to the view.
					foreach (var sd in SortDescriptions)
						_view.SortDescriptions.Add(sd);
					foreach (var gd in GroupDescriptions)
						_view.GroupDescriptions.Add(gd);
					if (SortDescriptions.Count > 0 || GroupDescriptions.Count > 0)
						_view.Refresh();
				}
				return _view;
			}
		}

		/// <summary>Sort descriptions applied to the view when it is created.</summary>
		public ObservableCollection<SortDescription> SortDescriptions { get; } =
			new ObservableCollection<SortDescription>();

		/// <summary>Group descriptions applied to the view when it is created.</summary>
		public ObservableCollection<GroupDescription> GroupDescriptions { get; } =
			new ObservableCollection<GroupDescription>();

		// ── Static helper ────────────────────────────────────────────────────────

		/// <summary>
		/// Returns a stable cached <see cref="ListCollectionView"/> for <paramref name="source"/>.
		/// Multiple calls with the same object return the same view instance.
		/// </summary>
		public static ICollectionView GetDefaultView(IEnumerable source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			// ICollectionView is its own view.
			if (source is ICollectionView cv) return cv;
			return _cache.GetValue(source, s => new ListCollectionView((IEnumerable)s));
		}
	}
}
