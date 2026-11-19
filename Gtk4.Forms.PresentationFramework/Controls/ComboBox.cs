using System;
using System.Collections.Generic;

namespace System.Windows.Controls;

/// <summary>
/// A drop-down selector backed by <see cref="Gtk.DropDown"/> with a <see cref="Gtk.StringList"/> model.
/// <para>
/// WPF derives from <c>Selector</c>; a shared Selector base is deferred, so this derives from
/// <see cref="ItemsControl"/> and keeps its own selection DPs. GtkDropDown auto-selects the first item of
/// a non-empty model, so "no selection" is only reflected in the widget while the model is empty. Rows are plain text (item text
/// from <see cref="ComboBoxItemText.GetText"/>), so <see cref="ItemsControl.ItemTemplate"/> visuals,
/// <see cref="UIElement"/> content, per-item <c>IsEnabled</c> and prefix auto-complete are not rendered.
/// </para>
/// </summary>
public partial class ComboBox : ItemsControl
{
	/// <summary>Raised when the selected item changes.</summary>
	public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

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
		var cb = (ComboBox)d;
		var idx = (int?)e.NewValue ?? -1;
		cb.ApplySelectedIndex(idx);
		// Without a widget no GTK notification drives SelectedItem.
		if (cb._dropDown == null && !cb._updating)
			cb.SetValue(SelectedItemProperty, idx >= 0 && idx < cb.Items.Count ? cb.Items[idx] : null);
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
		var idx = e.NewValue == null ? -1 : cb.Items.IndexOf(e.NewValue);
		if (idx >= 0 && cb.SelectedIndex != idx)
			cb.SetValue(SelectedIndexProperty, idx);

		// Fire SelectionChanged when SelectedItem is set directly (not from GTK notification).
		if (!cb._updating)
			cb.SelectionChanged?.Invoke(cb,
				new SelectionChangedEventArgs(cb, e.OldValue, e.NewValue));

		cb.RefreshSelectedValue();
		cb.OnSelectionChangedForText();
	}

	/// <summary>Gets or sets the selected item.</summary>
	public object? SelectedItem
	{
		get => GetValue(SelectedItemProperty);
		set => SetValue(SelectedItemProperty, value);
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
		RebuildStringList(); // push any items set before widget creation
		ApplySelectedIndex(SelectedIndex);
		return IsEditable ? CreateEditableHost() : _dropDown;
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
			var newItem = idx >= 0 && idx < Items.Count ? Items[idx] : null;
			SetValue(SelectedItemProperty, newItem);

			SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(this, oldItem, newItem));
		}
		finally { _updating = false; }
	}

	// ── Container hooks (WPF) ────────────────────────────────────────────────

	/// <inheritdoc/>
	protected override bool IsItemItsOwnContainerOverride(object? item) => item is ComboBoxItem;

	/// <inheritdoc/>
	protected override DependencyObject GetContainerForItemOverride() => new ComboBoxItem();
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