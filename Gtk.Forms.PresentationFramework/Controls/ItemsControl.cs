using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Data;

namespace System.Windows.Controls
{
	/// <summary>
	/// Base class for controls that display a collection of items.
	/// <para>
	/// Default backing widget is a vertical <see cref="Gtk.Box"/>; subclasses override
	/// <see cref="CreateItemsWidget"/>, <see cref="AddItemWidget"/>, and
	/// <see cref="RemoveItemWidget"/> to substitute a different GTK container.
	/// </para>
	/// <para>
	/// Each item produces a <see cref="UIElement"/> via <see cref="ItemTemplate"/> (when
	/// set) or <see cref="GetContainerForItem"/> (default: <see cref="ContentPresenter"/>).
	/// Reacts to <see cref="INotifyCollectionChanged"/> if the source implements it.
	/// </para>
	/// </summary>
	public class ItemsControl : Control
	{
		// ── ItemsSource DP ───────────────────────────────────────────────────────

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

		// ── ItemTemplate DP ──────────────────────────────────────────────────────

		/// <summary>Template used to generate each item's visual.</summary>
		public static readonly DependencyProperty ItemTemplateProperty =
			DependencyProperty.Register(
				nameof(ItemTemplate),
				typeof(DataTemplate),
				typeof(ItemsControl),
				new PropertyMetadata(null, OnItemTemplateChanged));

		static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ItemsControl)d).RebuildItems();
		}

		/// <summary>Gets or sets the item template.</summary>
		public DataTemplate? ItemTemplate
		{
			get => (DataTemplate?)GetValue(ItemTemplateProperty);
			set => SetValue(ItemTemplateProperty, value);
		}

		// ── Items list ───────────────────────────────────────────────────────────

		/// <summary>
		/// Snapshot of items. Updated on <see cref="ItemsSource"/> change or
		/// <see cref="INotifyCollectionChanged"/> notification.
		/// </summary>
		public IList<object?> Items { get; } = new List<object?>();

		// ── INCC subscription ────────────────────────────────────────────────────

		INotifyCollectionChanged? _incc;
		ICollectionView? _collectionView;

		void OnItemsSourceChanged(IEnumerable? oldSource, IEnumerable? newSource)
		{
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
				_incc.CollectionChanged += OnCollectionChanged;
			}

			RebuildItems();
		}

		void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			RebuildItems();
		}

		/// <summary>
		/// Returns the effective item sequence: the <see cref="ICollectionView"/> when available,
		/// otherwise the raw <see cref="ItemsSource"/>.
		/// </summary>
		IEnumerable EffectiveItems => (IEnumerable?)_collectionView ?? ItemsSource ?? Array.Empty<object>();

		// ── Item generation hooks (overridden by ListBox) ────────────────────────

		/// <summary>
		/// Creates the backing GTK widget that acts as the items container.
		/// Default: a vertical <see cref="Gtk.Box"/>. Override with a
		/// <see cref="Gtk.ListBox"/> (for <see cref="ListBox"/>) etc.
		/// </summary>
		protected virtual Gtk.Widget CreateItemsWidget()
		{
			_defaultBox = new Gtk.Box(Gtk.Orientation.Vertical, 0);
			return _defaultBox;
		}

		/// <summary>Adds an item's GTK widget to the items container.</summary>
		protected virtual void AddItemWidget(Gtk.Widget w) => _defaultBox!.Add(w);

		/// <summary>Removes an item's GTK widget from the items container.</summary>
		protected virtual void RemoveItemWidget(Gtk.Widget w) => _defaultBox!.Remove(w);

		Gtk.Box? _defaultBox;

		// Widgets currently packed by this control (parallel to Items).
		readonly List<(UIElement Container, Gtk.Widget Widget)> _rows =
			new List<(UIElement, Gtk.Widget)>();

		// ── Item factory ─────────────────────────────────────────────────────────

		/// <summary>
		/// Creates a container <see cref="UIElement"/> for <paramref name="item"/>.
		/// Default wraps in a <see cref="ContentPresenter"/>, or applies
		/// <see cref="ItemTemplate"/> if set.
		/// </summary>
		protected virtual UIElement GetContainerForItem(object? item)
		{
			if (ItemTemplate != null && ItemTemplate.HasContent)
			{
				var visual = ItemTemplate.CreateVisualTreeForItem(item);
				if (visual != null) return visual;
			}
			return new ContentPresenter { Content = item };
		}

		// ── GTK backing ──────────────────────────────────────────────────────────

		bool _widgetCreated;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			var w = CreateItemsWidget();
			_widgetCreated = true;
			PopulateRows();
			return w;
		}

		internal void RebuildItems()
		{
			Items.Clear();
			if (ItemsSource != null)
				foreach (var item in EffectiveItems)
					Items.Add(item);

			OnItemsRebuilt();

			if (!_widgetCreated) return;

			foreach (var (container, gw) in _rows)
			{
				// D1: clear logical parent when removing.
				if (container is UIElement uiContainer)
					uiContainer.LogicalParent = null;
				RemoveItemWidget(gw);
			}
			_rows.Clear();

			PopulateRows();
		}

		/// <summary>
		/// Called after <see cref="Items"/> has been refreshed (before GTK rows are rebuilt).
		/// Override in subclasses that need to react (e.g. reset selection).
		/// </summary>
		protected virtual void OnItemsRebuilt() { }

		void PopulateRows()
		{
			foreach (var item in Items)
			{
				var container = GetContainerForItem(item);
				// D1: wire logical-parent so TryFindResource walks the full tree.
				if (container is UIElement uiContainer)
					uiContainer.LogicalParent = this;
				var gw = container.Widget;
				_rows.Add((container, gw));
				AddItemWidget(gw);
			}
		}

		/// <summary>Returns the container <see cref="UIElement"/> at index <paramref name="i"/>.</summary>
		protected UIElement? ContainerAt(int i) =>
			i >= 0 && i < _rows.Count ? _rows[i].Container : null;
	}
}
