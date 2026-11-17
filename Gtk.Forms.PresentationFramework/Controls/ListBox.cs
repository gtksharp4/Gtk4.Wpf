using System.Collections.Generic;

namespace System.Windows.Controls;

/// <summary>
/// An items control with single-item selection, backed by <see cref="Gtk.ListBox"/>.
/// <para>
/// Each item is wrapped in a <see cref="ListBoxItem"/>. Selection maps to
/// <see cref="SelectedItem"/> / <see cref="SelectedIndex"/>;
/// <see cref="SelectionChanged"/> is raised on every change.
/// </para>
/// </summary>
public class ListBox : ItemsControl
{
	// ── SelectionChanged event ────────────────────────────────────────────────

	/// <summary>Raised when <see cref="SelectedItem"/> or <see cref="SelectedIndex"/> changes.</summary>
	public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

	// ── SelectedIndex DP ──────────────────────────────────────────────────────

	/// <summary>Zero-based index of the selected item; -1 when nothing selected.</summary>
	public static readonly DependencyProperty SelectedIndexProperty =
		DependencyProperty.Register(
			nameof(SelectedIndex),
			typeof(int),
			typeof(ListBox),
			new PropertyMetadata(-1, OnSelectedIndexChanged));

	static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ListBox)d).ApplySelectedIndex((int?)e.NewValue ?? -1);
	}

	/// <summary>Gets or sets the selected index.</summary>
	public int SelectedIndex
	{
		get => (int?)GetValue(SelectedIndexProperty) ?? -1;
		set => SetValue(SelectedIndexProperty, value);
	}

	// ── SelectedItem DP ───────────────────────────────────────────────────────

	/// <summary>The currently selected item.</summary>
	public static readonly DependencyProperty SelectedItemProperty =
		DependencyProperty.Register(
			nameof(SelectedItem),
			typeof(object),
			typeof(ListBox),
			new PropertyMetadata(null, OnSelectedItemChanged));

	static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ListBox)d).ApplySelectedItem(e.OldValue, e.NewValue);
	}

	/// <summary>Gets or sets the selected item.</summary>
	public object? SelectedItem
	{
		get => GetValue(SelectedItemProperty);
		set => SetValue(SelectedItemProperty, value);
	}

	// ── GTK backing ──────────────────────────────────────────────────────────

	Gtk.ListBox? _gtkListBox;
	readonly List<ListBoxItem> _containers = new List<ListBoxItem>();

	/// <summary>
	/// Re-entrant guard: when set, DP callbacks and GTK handlers suppress
	/// further cross-sync and do NOT raise SelectionChanged a second time.
	/// </summary>
	bool _updating;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateItemsWidget()
	{
		_gtkListBox = new Gtk.ListBox();
		_gtkListBox.SelectionMode = Gtk.SelectionMode.Single;
		_gtkListBox.RowSelected += OnGtkRowSelected;
		return _gtkListBox;
	}

	/// <inheritdoc/>
	protected override UIElement GetContainerForItem(object? item)
	{
		var lbi = new ListBoxItem { Content = item };
		_containers.Add(lbi);
		return lbi;
	}

	/// <inheritdoc/>
	protected override void AddItemWidget(Gtk.Widget w) => _gtkListBox!.Append(w);

	/// <inheritdoc/>
	protected override void RemoveItemWidget(Gtk.Widget w) => _gtkListBox!.Remove(w);

	/// <inheritdoc/>
	protected override void OnItemsRebuilt()
	{
		_containers.Clear();

		// U1: preserve selection across INCC-triggered rebuilds.
		// If the previously selected item is still present in Items, restore it;
		// only clear when it has genuinely left the collection.
		var prevItem = SelectedItem;
		var prevIdx = -1;
		if (prevItem != null)
		{
			for (int i = 0; i < Items.Count; i++)
			{
				if (Equals(Items[i], prevItem)) { prevIdx = i; break; }
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

	// ── GTK row-selected handler ──────────────────────────────────────────────

	void OnGtkRowSelected(object? sender, Gtk.RowSelectedArgs e)
	{
		if (_updating) return;

		var row = e.Row;
		var oldItem = SelectedItem;

		if (row == null)
		{
			SetSelectionCore(-1, null, oldItem);
			return;
		}

		int idx = -1;
		for (int i = 0; i < _containers.Count; i++)
		{
			if (ReferenceEquals(_containers[i].Row, row)) { idx = i; break; }
		}
		if (idx < 0) return;

		var newItem = idx < Items.Count ? Items[idx] : null;
		SetSelectionCore(idx, newItem, oldItem);
	}

	// ── DP callbacks ─────────────────────────────────────────────────────────

	/// <summary>
	/// D4/D5: Entry point when <see cref="SelectedIndex"/> is set programmatically.
	/// Cross-syncs SelectedItem and GTK row; raises SelectionChanged exactly once.
	/// </summary>
	void ApplySelectedIndex(int idx)
	{
		if (_updating) return;

		object? newItem = (idx >= 0 && idx < Items.Count) ? Items[idx] : null;
		if (idx < 0 || idx >= Items.Count) idx = -1;
		var oldItem = SelectedItem;

		// No-op if nothing actually changed.
		if (idx == SelectedIndex && Equals(newItem, oldItem)) return;

		SetSelectionCore(idx, newItem, oldItem);
	}

	/// <summary>
	/// D4/D5: Entry point when <see cref="SelectedItem"/> is set programmatically.
	/// Cross-syncs SelectedIndex and GTK row; raises SelectionChanged exactly once.
	/// </summary>
	void ApplySelectedItem(object? oldItem, object? newItem)
	{
		if (_updating) return;

		int idx = -1;
		for (int i = 0; i < Items.Count; i++)
		{
			if (Equals(Items[i], newItem)) { idx = i; break; }
		}

		// D5: if newItem not in list, treat as deselect.
		if (newItem != null && idx < 0)
		{
			// item not in list — set index -1 only if it was different; no event if nothing changed
			if (SelectedIndex == -1 && SelectedItem == null) return;
			SetSelectionCore(-1, null, oldItem);
			return;
		}

		// No-op if nothing changed.
		if (idx == SelectedIndex && Equals(newItem, oldItem)) return;

		SetSelectionCore(idx, newItem, oldItem);
	}

	// ── Central selection mutator (D4: raises exactly once) ───────────────────

	/// <summary>
	/// Updates both DPs and the GTK row selection, then raises
	/// <see cref="SelectionChanged"/> exactly once.
	/// </summary>
	void SetSelectionCore(int idx, object? newItem, object? oldItem)
	{
		_updating = true;
		try
		{
			SetValue(SelectedIndexProperty, idx);
			SetValue(SelectedItemProperty, newItem);

			// D5: sync GTK visual selection.
			if (_gtkListBox != null)
			{
				if (idx < 0 || idx >= _containers.Count)
				{
					_gtkListBox.UnselectAll();
				}
				else
				{
					var row = _containers[idx].Row;
					if (row != null) _gtkListBox.SelectRow(row);
				}
			}
		}
		finally { _updating = false; }

		// D4: raise exactly once, after all state is consistent.
		SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(this, oldItem, newItem));
	}
}