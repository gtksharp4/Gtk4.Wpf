using System.Collections.Generic;

namespace System.Windows.Controls;

/// <summary>
/// A node in a <see cref="TreeView"/>, backed by a <see cref="Gtk.Expander"/> that
/// contains a vertical <see cref="Gtk.Box"/> of child item widgets.
/// <para>
/// GTK-widget choice: <c>Gtk.TreeExpander</c>+<c>Gtk.ListView</c> requires a full
/// <c>Gio.ListModel</c> tree model. A recursive <c>Gtk.Expander</c>/<c>Gtk.Box</c>
/// tree is simpler, faithful for this phase, and directly hosts UIElement children.
/// </para>
/// </summary>
public partial class TreeViewItem : HeaderedItemsControl
{
	/// <summary>Raised when <see cref="IsSelected"/> changes.</summary>
	public event EventHandler? Selected;

	/// <summary>Raised when <see cref="IsSelected"/> becomes <c>false</c>.</summary>
	public event EventHandler? Unselected;

	// ── IsExpanded DP ─────────────────────────────────────────────────────────

	/// <summary><c>true</c> when child items are visible.</summary>
	public static readonly DependencyProperty IsExpandedProperty =
		DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(TreeViewItem),
			new PropertyMetadata(false, OnIsExpandedChanged));

	static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var tvi = (TreeViewItem)d;
		if (tvi._expander == null) return; // widget not yet created — applied in CreateWidget
		var val = (bool?)e.NewValue == true;
		if (tvi._expander.Expanded != val)
		{
			tvi._updating = true;
			try { tvi._expander.Expanded = val; }
			finally { tvi._updating = false; }
		}
	}

	/// <summary>Gets or sets whether child items are expanded.</summary>
	public bool IsExpanded
	{
		get => (bool?)GetValue(IsExpandedProperty) == true;
		set => SetValue(IsExpandedProperty, value);
	}

	// ── IsSelected DP ─────────────────────────────────────────────────────────

	/// <summary><c>true</c> when this item is the selected item in its <see cref="TreeView"/>.</summary>
	public static readonly DependencyProperty IsSelectedProperty =
		DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(TreeViewItem),
			new FrameworkPropertyMetadata(false,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSelectedChanged));

	static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var tvi = (TreeViewItem)d;
		if ((bool?)e.NewValue == true)
			tvi.Selected?.Invoke(tvi, EventArgs.Empty);
		else
			tvi.Unselected?.Invoke(tvi, EventArgs.Empty);
	}

	/// <summary>Gets or sets whether this item is selected.</summary>
	public bool IsSelected
	{
		get => (bool?)GetValue(IsSelectedProperty) == true;
		set => SetValue(IsSelectedProperty, value);
	}

	// ── GTK backing ───────────────────────────────────────────────────────────

	Gtk.Expander? _expander;
	Gtk.Box? _childBox;
	bool _updating;
	readonly List<(UIElement Container, Gtk.Widget Widget)> _childWidgets =
		new List<(UIElement, Gtk.Widget)>();

	/// <inheritdoc/>
	protected override Gtk.Widget CreateItemsWidget()
	{
		_expander = new Gtk.Expander(HeaderText);
		if (ResolveHeaderWidget() is { } headerWidget) _expander.LabelWidget = headerWidget;
		_expander.Expanded = IsExpanded;
		_childBox = new Gtk.Box(Gtk.Orientation.Vertical, 2);
		_expander.Child = _childBox;
		_expander.AddNotification("expanded", OnGtkNotifyExpanded);
		return _expander;
	}

	/// <inheritdoc/>
	protected override void OnHeaderChanged(object? oldHeader, object? newHeader)
	{
		if (_expander == null) return;
		var widget = ResolveHeaderWidget();
		if (widget != null)
			_expander.LabelWidget = widget;
		else
			_expander.Label = HeaderText;
	}

	/// <inheritdoc/>
	protected override void AddItemWidget(Gtk.Widget w) => _childBox!.Add(w);

	/// <inheritdoc/>
	protected override void RemoveItemWidget(Gtk.Widget w) => _childBox!.Remove(w);

	void OnGtkNotifyExpanded(object? sender, GLib.NotifyArgs e)
	{
		if (e.Property != "expanded") return;
		if (_updating) return;
		_updating = true;
		try { SetValue(IsExpandedProperty, _expander!.Expanded); }
		finally { _updating = false; }
	}
}