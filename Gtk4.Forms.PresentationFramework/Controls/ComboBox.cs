using System;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

/// <summary>
/// A drop-down selector backed by <see cref="Gtk.DropDown"/> with a <see cref="Gtk.StringList"/> model.
/// <para>
/// WPF derives from <c>Selector</c>; a shared Selector base is deferred, so this derives from
/// <see cref="ItemsControl"/> and keeps its own selection DPs. GtkDropDown cannot show "no selection" while its
/// model is non-empty: it keeps the last shown (or the first) item, so <see cref="SelectedIndex"/> == -1 is only
/// reflected in the DPs. Picking the item the box already shows then fires no GTK notification, so it cannot be
/// selected until another item is picked. Rows are plain text (item text
/// from <see cref="ComboBoxItemText.GetText"/>), so <see cref="ItemsControl.ItemTemplate"/> visuals,
/// <see cref="UIElement"/> content, per-item <c>IsEnabled</c> and prefix auto-complete are not rendered.
/// </para>
/// </summary>
public partial class ComboBox : Selector
{
	internal override DependencyProperty SelectedItemDependencyProperty => SelectedItemProperty;

	// ── SelectedIndex DP ─────────────────────────────────────────────────────

	/// <summary>Zero-based index of the selected item; -1 when nothing is selected.</summary>
	public static readonly DependencyProperty SelectedIndexProperty =
		DependencyProperty.Register(
			nameof(SelectedIndex),
			typeof(int),
			typeof(ComboBox),
			new FrameworkPropertyMetadata(-1,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
				OnSelectedIndexChanged),
			v => (int)v! >= -1);

	static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var cb = (ComboBox)d;
		if (cb._rejecting) return;
		var idx = (int?)e.NewValue ?? -1;
		// WPF ignores an index past the end of a non-empty list; an empty list keeps it pending.
		if (!cb._updating && idx >= cb.Items.Count && cb.Items.Count > 0)
		{
			cb.Reject(SelectedIndexProperty, e.OldValue);
			return;
		}
		cb.ApplySelectedIndex(idx);
		// Without a widget no GTK notification drives SelectedItem; with one, GTK stays silent if it already shows idx.
		if (cb._updating) return;
		if (cb._dropDown != null)
		{
			if (idx < 0)
				cb.SetValue(SelectedItemProperty, null);
			else cb.SyncSelectedItemToIndex();
		}
		else
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
		if (cb._rejecting) return;
		var idx = e.NewValue == null ? -1 : cb.Items.IndexOf(e.NewValue);
		// WPF rejects an item that is not in Items; null still clears.
		if (idx < 0 && e.NewValue != null && !cb._updating)
		{
			cb.Reject(SelectedItemProperty, e.OldValue);
			return;
		}
		if (cb.SelectedIndex != idx)
			cb.SetValue(SelectedIndexProperty, idx);

		// Fire SelectionChanged when SelectedItem is set directly (not from GTK notification).
		if (!cb._updating)
			cb.NotifySelectionChanged(e.OldValue, e.NewValue);

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
	bool _rejecting;

	/// <summary>Restores a rejected selection value without side effects or <see cref="SelectionChanged"/>.</summary>
	void Reject(DependencyProperty property, object? oldValue)
	{
		_rejecting = true;
		try { SetValue(property, oldValue); }
		finally { _rejecting = false; }
	}

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_stringList = new Gtk.StringList(Array.Empty<string>());
		_dropDown = new Gtk.DropDown(null, null);
		_dropDown.Model = _stringList;
		_dropDown.AddNotification("selected", OnGtkSelectionChanged);
		RebuildStringList(); // push any items set before widget creation
		_updating = true; // GTK echoes the initial index back; WPF raises no SelectionChanged for it
		try { ApplySelectedIndex(SelectedIndex); }
		finally { _updating = false; }
		SyncSelectedItemToIndex();
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

			NotifySelectionChanged(oldItem, newItem);
		}
		finally { _updating = false; }
	}

	// ── Container hooks (WPF) ────────────────────────────────────────────────

	/// <inheritdoc/>
	protected override bool IsItemItsOwnContainerOverride(object? item) => item is ComboBoxItem;

	/// <inheritdoc/>
	protected override DependencyObject GetContainerForItemOverride() => new ComboBoxItem();
}
