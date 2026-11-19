using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Controls;

/// <summary>
/// A drop-down selector backed by <see cref="Gtk.DropDown"/> with a
/// <see cref="Gtk.StringList"/> model.
/// <para>
/// GTK4-native <c>Gtk.DropDown</c> is used (not the deprecated <c>Gtk.ComboBoxText</c>).
/// Items are converted to string via <c>ToString()</c>; object identity is tracked
/// so <see cref="SelectedItem"/> returns the original object.
/// </para>
/// </summary>
public partial class ComboBox : Control
{
	// ── SelectionChanged event ───────────────────────────────────────────────

	/// <summary>Raised when the selected item changes.</summary>
	public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

	// ── ItemsSource DP ───────────────────────────────────────────────────────

	/// <summary>The collection of items to display.</summary>
	public static readonly DependencyProperty ItemsSourceProperty =
		DependencyProperty.Register(
			nameof(ItemsSource),
			typeof(IList),
			typeof(ComboBox),
			new PropertyMetadata(null, OnItemsSourceChanged));

	static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ComboBox)d).RebuildItems();
	}

	/// <summary>Gets or sets the items source.</summary>
	public IList? ItemsSource
	{
		get => (IList?)GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}

	// ── SelectedIndex DP ─────────────────────────────────────────────────────

	/// <summary>Zero-based index of the selected item; -1 when nothing is selected.</summary>
	public static readonly DependencyProperty SelectedIndexProperty =
		DependencyProperty.Register(
			nameof(SelectedIndex),
			typeof(int),
			typeof(ComboBox),
			new FrameworkPropertyMetadata(-1,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnSelectedIndexChanged));

	static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ComboBox)d).ApplySelectedIndex((int?)e.NewValue ?? -1);
	}

	/// <summary>Gets or sets the selected index.</summary>
	public int SelectedIndex
	{
		get => (int?)GetValue(SelectedIndexProperty) ?? -1;
		set => SetValue(SelectedIndexProperty, value);
	}

	// ── SelectedItem DP ──────────────────────────────────────────────────────

	/// <summary>The currently selected item.</summary>
	public static readonly DependencyProperty SelectedItemProperty =
		DependencyProperty.Register(
			nameof(SelectedItem),
			typeof(object),
			typeof(ComboBox),
			new FrameworkPropertyMetadata(null,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

	static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var cb = (ComboBox)d;
		var idx = e.NewValue == null ? -1 : cb._items.IndexOf(e.NewValue);
		if (idx >= 0 && cb.SelectedIndex != idx)
			cb.SetValue(SelectedIndexProperty, idx);

		// Fire SelectionChanged when SelectedItem is set directly (not from GTK notification).
		if (!cb._updating)
			cb.SelectionChanged?.Invoke(cb,
				new SelectionChangedEventArgs(cb, e.OldValue, e.NewValue));

		cb.RefreshSelectedValue();
	}

	/// <summary>Gets or sets the selected item.</summary>
	public object? SelectedItem
	{
		get => GetValue(SelectedItemProperty);
		set => SetValue(SelectedItemProperty, value);
	}

	// ── Item storage ─────────────────────────────────────────────────────────

	readonly List<object> _items = new List<object>();

	void RebuildItems()
	{
		_items.Clear();
		if (ItemsSource != null)
			foreach (var item in ItemsSource)
				_items.Add(item);

		if (_dropDown == null) return;

		// Suppress GTK "selected" notifications while we swap the model.
		_updating = true;
		try
		{
			_stringList = new Gtk.StringList(_items.Count > 0
				? _items.ConvertAll(i => i?.ToString() ?? string.Empty).ToArray()
				: Array.Empty<string>());
			_dropDown.Model = _stringList;
		}
		finally { _updating = false; }

		// Re-apply any SelectedIndex that was set before the widget was created.
		var currentIdx = SelectedIndex;
		if (currentIdx >= 0 && currentIdx < _items.Count)
			ApplySelectedIndex(currentIdx);
		else
		{
			SetValue(SelectedIndexProperty, -1);
			SetValue(SelectedItemProperty, null);
		}
	}

	void ApplySelectedIndex(int index)
	{
		if (_dropDown == null) return;
		if (index < 0 || index >= _items.Count)
		{
			_dropDown.Selected = uint.MaxValue; // GTK_INVALID_LIST_POSITION
			return;
		}
		if (_dropDown.Selected != (uint)index)
			_dropDown.Selected = (uint)index;
	}

	// ── GTK backing ──────────────────────────────────────────────────────────

	Gtk.DropDown _dropDown = null!;
	Gtk.StringList _stringList = null!;
	bool _updating;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_stringList = new Gtk.StringList(Array.Empty<string>());
		_dropDown = new Gtk.DropDown(null, null);
		_dropDown.Model = _stringList;
		_dropDown.AddNotification("selected", OnGtkSelectionChanged);
		RebuildItems(); // push any items set before widget creation
		return _dropDown;
	}

	void OnGtkSelectionChanged(object? sender, GLib.NotifyArgs e)
	{
		if (_updating) return;
		_updating = true;
		try
		{
			var gtkIdx = (int)_dropDown.Selected;
			// GTK_INVALID_LIST_POSITION == uint.MaxValue
			var idx = gtkIdx == unchecked((int)uint.MaxValue) ? -1 : gtkIdx;

			var oldItem = SelectedItem;
			SetValue(SelectedIndexProperty, idx);
			var newItem = idx >= 0 && idx < _items.Count ? _items[idx] : null;
			SetValue(SelectedItemProperty, newItem);

			SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(this, oldItem, newItem));
		}
		finally { _updating = false; }
	}
}

/// <summary>Event data for <see cref="ComboBox.SelectionChanged"/> and <see cref="ListBox.SelectionChanged"/>.</summary>
public sealed partial class SelectionChangedEventArgs : EventArgs
{
	/// <summary>The control whose selection changed (a <see cref="ComboBox"/> or <see cref="ListBox"/>).</summary>
	public object Source { get; }
	/// <summary>The previously selected item.</summary>
	public object? OldValue { get; }
	/// <summary>The newly selected item.</summary>
	public object? NewValue { get; }

	internal SelectionChangedEventArgs(object source, object? oldValue, object? newValue)
	{
		Source = source;
		OldValue = oldValue;
		NewValue = newValue;
		var added = new System.Collections.Generic.List<object?>();
		var removed = new System.Collections.Generic.List<object?>();
		if (newValue != null) added.Add(newValue);
		if (oldValue != null) removed.Add(oldValue);
		AddedItems = added;
		RemovedItems = removed;
	}
}