using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>
/// A tab-strip control backed by <see cref="Gtk.Notebook"/>.
/// Children must be <see cref="TabItem"/> instances.
/// </summary>
public partial class TabControl : Selector
{
	internal override DependencyProperty SelectedItemDependencyProperty => SelectedItemProperty;

	// ── SelectedIndex DP ────────────────────────────────────────────────────

	/// <summary>Zero-based index of the selected tab; -1 when nothing selected.</summary>
	public static readonly DependencyProperty SelectedIndexProperty =
		DependencyProperty.Register(
			nameof(SelectedIndex),
			typeof(int),
			typeof(TabControl),
			new FrameworkPropertyMetadata(-1,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnSelectedIndexChanged));

	static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var tc = (TabControl)d;
		int idx = (int?)e.NewValue ?? -1;
		if (tc._updating)
		{
			// Re-entrant (from OnSelectedItemChanged or OnGtkSwitchPage): just sync notebook.
			tc.ApplySelectedIndexDirect(idx);
			return;
		}
		tc._updating = true;
		try
		{
			tc.ApplySelectedIndexDirect(idx);
			var oldItem = tc.SelectedItem;
			var newItem = idx >= 0 && idx < tc.Items.Count ? tc.Items[idx] : null;
			if (!ReferenceEquals(oldItem, newItem))
				tc.SetValue(SelectedItemProperty, newItem);
			tc.NotifySelectionChanged(oldItem, newItem);
		}
		finally { tc._updating = false; }
	}

	/// <summary>Gets or sets the selected tab index.</summary>
	public int SelectedIndex
	{
		get => (int?)GetValue(SelectedIndexProperty) ?? -1;
		set => SetValue(SelectedIndexProperty, value);
	}

	// ── SelectedItem DP ─────────────────────────────────────────────────────

	/// <summary>The currently selected <see cref="TabItem"/>.</summary>
	public static readonly DependencyProperty SelectedItemProperty =
		DependencyProperty.Register(
			nameof(SelectedItem),
			typeof(object),
			typeof(TabControl),
			new FrameworkPropertyMetadata(null,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

	static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var tc = (TabControl)d;
		if (tc._updating) return;
		tc._updating = true;
		try
		{
			var idx = e.NewValue == null ? -1 : tc.Items.IndexOf(e.NewValue);
			if (idx >= 0 && tc.SelectedIndex != idx)
				tc.SetValue(SelectedIndexProperty, idx);
			// Apply to notebook directly; GTK SwitchPage re-entry is blocked by _updating.
			tc.ApplySelectedIndexDirect(idx < 0 ? -1 : idx);
			// Single raise for programmatic path.
			tc.NotifySelectionChanged(e.OldValue, e.NewValue);
		}
		finally { tc._updating = false; }
	}

	/// <summary>Gets or sets the selected tab item.</summary>
	public object? SelectedItem
	{
		get => GetValue(SelectedItemProperty);
		set => SetValue(SelectedItemProperty, value);
	}

	// ── GTK backing ─────────────────────────────────────────────────────────

	Gtk.Notebook? _notebook;
	readonly List<TabItem> _tabItems = new List<TabItem>();
	bool _updating;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateItemsWidget()
	{
		_notebook = new Gtk.Notebook();
		_notebook.SwitchPage += OnGtkSwitchPage;
		_notebook.TabPos = ToPosition(TabStripPlacement);
		return _notebook;
	}

	/// <inheritdoc/>
	protected override UIElement GetContainerForItem(object? item)
	{
		var ti = item as TabItem ?? CreateTabFor(item);
		_tabItems.Add(ti);
		return ti;
	}

	/// <inheritdoc/>
	protected override void AddItemWidget(Gtk.Widget w)
	{
		var ti = _tabItems.Count > 0 ? _tabItems[_tabItems.Count - 1] : null;
		var tabLabel = ti?.CreateTabWidget() ?? new Gtk.Label(string.Empty);
		_notebook!.AppendPage(w, tabLabel);
	}

	/// <inheritdoc/>
	protected override void RemoveItemWidget(Gtk.Widget w)
	{
		int pageNum = _notebook!.PageNum(w);
		if (pageNum >= 0)
			_notebook.RemovePage(pageNum);
	}

	/// <inheritdoc/>
	protected override void OnItemsRebuilt()
	{
		_tabItems.Clear();

		// U1: preserve selection across INCC-triggered rebuilds.
		var prevItem = SelectedItem;
		var prevIdx = -1;
		if (prevItem != null)
		{
			for (int i = 0; i < Items.Count; i++)
			{
				if (ReferenceEquals(Items[i], prevItem) || Equals(Items[i], prevItem))
				{ prevIdx = i; break; }
			}
		}

		_updating = true;
		try
		{
			SetValue(SelectedIndexProperty, prevIdx);
			SetValue(SelectedItemProperty, prevIdx >= 0 ? prevItem : null);
		}
		finally { _updating = false; }
	}

	void ApplySelectedIndexDirect(int idx)
	{
		if (_notebook == null) return;
		if (idx >= 0 && idx < Items.Count && _notebook.CurrentPage != idx)
			_notebook.CurrentPage = idx;
	}

	void OnGtkSwitchPage(object? sender, Gtk.SwitchPageArgs e)
	{
		if (_updating) return;
		_updating = true;
		try
		{
			var idx = (int)e.PageNum;
			var oldItem = SelectedItem;
			SetValue(SelectedIndexProperty, idx);
			var newItem = idx >= 0 && idx < Items.Count ? Items[idx] : null;
			SetValue(SelectedItemProperty, newItem);
			NotifySelectionChanged(oldItem, newItem);
		}
		finally { _updating = false; }
	}
}
