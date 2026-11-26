using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;

namespace System.Windows.Controls;

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
[ContentProperty("Items")]
public partial class ItemsControl : Control
{
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
	/// The items: the view of <see cref="ItemsSource"/> when set, otherwise the inline items
	/// (observable; mutating it while <see cref="ItemsSource"/> is in use throws).
	/// </summary>
	public ItemCollection Items => _items ??= new ItemCollection(this);

	// ── Item generation hooks (overridden by ListBox) ────────────────────────

	/// <summary>
	/// Creates the backing GTK widget that acts as the items container.
	/// Default: a vertical <see cref="Gtk.Box"/>. Override with a
	/// <see cref="Gtk.ListBox"/> (for <see cref="ListBox"/>) etc.
	/// </summary>
	protected virtual Gtk.Widget CreateItemsWidget()
	{
		_defaultBox = new Gtk.Box(Gtk.Orientation.Vertical, 0);
		InstallItemsPanel();
		return _defaultBox;
	}

	/// <summary>Adds an item's GTK widget to the items container.</summary>
	protected virtual void AddItemWidget(Gtk.Widget w) => AddDefaultHostItem(w);

	/// <summary>Removes an item's GTK widget from the items container.</summary>
	protected virtual void RemoveItemWidget(Gtk.Widget w) => RemoveDefaultHostItem(w);

	Gtk.Box? _defaultBox;

	// Widgets currently packed by this control (parallel to Items).
	readonly List<(UIElement Container, Gtk.Widget Widget)> _rows =
		new List<(UIElement, Gtk.Widget)>();

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

	/// <summary>True while the items are being rebuilt; derived selection changes are then not user choices.</summary>
	internal bool IsRebuildingItems { get; private set; }

	internal void RebuildItems()
	{
		bool wasRebuilding = IsRebuildingItems;
		IsRebuildingItems = true;
		try { RebuildCore(); }
		finally { IsRebuildingItems = wasRebuilding; }
		if (!wasRebuilding)
		{
			OnItemsRebuildCompleted();
			ExistingAutomationPeer?.NotifyChildrenChanged();
		}
	}

	/// <summary>Called once a rebuild has repopulated the rows and the selection reconciled itself.</summary>
	internal virtual void OnItemsRebuildCompleted() { }

	void RebuildCore()
	{
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
		SetGeneratorStatus(GeneratorStatus.GeneratingContainers);
		foreach (var item in Items)
		{
			var container = GetContainerForItem(item);
			ApplyItemBindingGroup(container);
			// D1: wire logical-parent so TryFindResource walks the full tree.
			if (container is UIElement uiContainer)
				uiContainer.LogicalParent = this;
			var gw = container.Widget;
			int idx = _rows.Count;
			_rows.Add((container, gw));
			OnContainerAdded(container, idx);
			AddItemWidget(gw);
		}
		SetGeneratorStatus(GeneratorStatus.ContainersGenerated);
	}

	/// <summary>Returns the container <see cref="UIElement"/> at index <paramref name="i"/>.</summary>
	protected internal UIElement? ContainerAt(int i) =>
		i >= 0 && i < _rows.Count ? _rows[i].Container : null;
}