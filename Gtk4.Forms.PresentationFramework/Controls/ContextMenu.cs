namespace System.Windows.Controls;

/// <summary>
/// A floating context menu backed by a <see cref="Gtk.Popover"/> that contains
/// a vertical <see cref="Gtk.Box"/> of item widgets.
/// <para>
/// GTK-widget choice: <c>Gtk.PopoverMenu</c> requires a <c>Gio.Menu</c> model; a plain
/// <c>Gtk.Popover</c> with a vertical <c>Gtk.Box</c> hosts arbitrary child widgets.
/// Full <c>Gtk.PopoverMenu</c> support (keyboard navigation, accelerators) is deferred.
/// </para>
/// <para>
/// Attach to any <see cref="FrameworkElement"/> via
/// <see cref="FrameworkElement.ContextMenu"/>; calling <see cref="Open"/> with a
/// placement widget shows the popover anchored to that widget.
/// </para>
/// </summary>
public partial class ContextMenu : ItemsControl
{
	// ── IsOpen DP ─────────────────────────────────────────────────────────────

	/// <summary><c>true</c> while the context menu is visible.</summary>
	public static readonly DependencyProperty IsOpenProperty =
		DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(ContextMenu),
			new FrameworkPropertyMetadata(false,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));

	static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var cm = (ContextMenu)d;
		if (cm._updating) return;
		var open = (bool?)e.NewValue == true;
		// Widget not yet materialised: remember intent; replay at OnWidgetCreated.
		if (!cm.IsWidgetCreated)
		{
			cm._pendingOpen = open;
			return;
		}
		if (open) cm.ShowPopover(); else cm.HidePopover();
	}

	/// <summary>Gets or sets whether the context menu is currently open.</summary>
	public bool IsOpen
	{
		get => (bool?)GetValue(IsOpenProperty) == true;
		set => SetValue(IsOpenProperty, value);
	}

	// ── Placement DP ──────────────────────────────────────────────────────────

	/// <summary>
	/// Placement hint; stored for future use. The current implementation always
	/// anchors to the parent widget passed to <see cref="Open"/>.
	/// </summary>
	public static readonly DependencyProperty PlacementProperty =
		DependencyProperty.Register(nameof(Placement), typeof(PlacementMode), typeof(ContextMenu),
			new PropertyMetadata(PlacementMode.MousePoint));

	/// <summary>Gets or sets the placement mode (stored; not yet forwarded to GTK).</summary>
	public PlacementMode Placement
	{
		get => (PlacementMode)(GetValue(PlacementProperty) ?? PlacementMode.MousePoint);
		set => SetValue(PlacementProperty, value);
	}

	// ── GTK backing ───────────────────────────────────────────────────────────

	Gtk.Box? _itemsBox;
	Gtk.Popover? _popover;
	bool _updating;
	/// <summary>
	/// Remembers an <see cref="IsOpen"/> assignment made before the backing widget was
	/// created. Consumed and cleared in <see cref="OnWidgetCreated"/>.
	/// </summary>
	internal bool _pendingOpen;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateItemsWidget()
	{
		_itemsBox = new Gtk.Box(Gtk.Orientation.Vertical, 2);
		_popover = new Gtk.Popover { Child = _itemsBox };
		_popover.Closed += OnPopoverClosed;
		// Return the box as the "items widget"; the popover wraps it.
		return _itemsBox;
	}

	// Widget is the items box; callers wanting the Popover access it via GtkPopover.
	/// <summary>Returns the underlying <see cref="Gtk.Popover"/> (GTK smoke tests).</summary>
	internal Gtk.Popover? GtkPopover => _popover;

	/// <inheritdoc/>
	protected override void AddItemWidget(Gtk.Widget w) => _itemsBox!.Add(w);

	/// <inheritdoc/>
	protected override void RemoveItemWidget(Gtk.Widget w) => _itemsBox!.Remove(w);

	/// <summary>
	/// Opens the context menu anchored to <paramref name="parentWidget"/>.
	/// Sets <see cref="IsOpen"/> = <c>true</c>.
	/// </summary>
	public void Open(Gtk.Widget parentWidget)
	{
		// Ensure the widget tree is built.
		_ = Widget;
		if (_popover == null) return;
		if (_popover.Parent is { } current && current.Handle != parentWidget.Handle) _popover.Unparent();
		if (_popover.Parent == null) _popover.Parent = parentWidget;
		ShowPopover();
	}

	void ShowPopover()
	{
		if (_popover == null) return;
		_popover.Popup();
		_updating = true;
		try { SetValue(IsOpenProperty, true); }
		finally { _updating = false; }
	}

	void HidePopover()
	{
		_popover?.Popdown();
	}

	void OnPopoverClosed(object? sender, EventArgs e)
	{
		_updating = true;
		try { SetValue(IsOpenProperty, false); }
		finally { _updating = false; }
		OnClosedCore();
	}

	/// <inheritdoc/>
	protected override void OnWidgetCreated(Gtk.Widget widget)
	{
		base.OnWidgetCreated(widget);
		// Replay any IsOpen=true assignment that arrived before the backing widget existed.
		if (_pendingOpen)
		{
			_pendingOpen = false;
			ShowPopover();
		}
	}
}

/// <summary>Placement modes for <see cref="ContextMenu"/> and <see cref="Popup"/>.</summary>
public enum PlacementMode
{
	/// <summary>Near the mouse cursor.</summary>
	MousePoint,
	/// <summary>Below the placement target.</summary>
	Bottom,
	/// <summary>Above the placement target.</summary>
	Top,
	/// <summary>Right of the placement target.</summary>
	Right,
	/// <summary>Left of the placement target.</summary>
	Left,
	/// <summary>Relative to the placement target rectangle.</summary>
	Relative,
	/// <summary>At absolute screen coordinates specified by <c>HorizontalOffset</c> / <c>VerticalOffset</c>.</summary>
	Absolute,
	/// <summary>At the absolute point specified by <c>HorizontalOffset</c> / <c>VerticalOffset</c> (top-left anchor).</summary>
	AbsolutePoint,
	/// <summary>Centered over the placement target.</summary>
	Center,
	/// <summary>Application-defined custom placement; falls back to <see cref="Bottom"/>.</summary>
	Custom,
	/// <summary>Near the mouse cursor position (alias of <see cref="MousePoint"/> for WPF compat).</summary>
	Mouse,
	/// <summary>At a point relative to the placement target, specified by <c>HorizontalOffset</c> / <c>VerticalOffset</c>.</summary>
	RelativePoint,
}