namespace System.Windows;

/// <summary>
/// <c>AllowDrop</c> dependency property and WPF drag routed events
/// (tunnel/bubble pairs: Preview+plain for DragEnter/DragOver/DragLeave/Drop).
/// GTK <c>Gtk.DropTarget</c> wiring lives in <c>UIElement.GtkDragDrop.cs</c>.
/// </summary>
/// <remarks>
/// <b>Headless:</b> events are declared and raisable via <see cref="RaiseEvent"/> in all
/// environments. The GTK wiring that fires them from real OS gestures requires a display
/// (see <c>UIElement.GtkDragDrop.cs</c>).
/// </remarks>
public abstract partial class UIElement
{
	// ── AllowDrop DP ─────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="AllowDrop"/> dependency property.
	/// Default is <see langword="false"/>.
	/// </summary>
	public static readonly DependencyProperty AllowDropProperty =
		DependencyProperty.Register(
			nameof(AllowDrop),
			typeof(bool),
			typeof(UIElement),
			new PropertyMetadata(false, OnAllowDropChanged));

	static void OnAllowDropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is UIElement el && el.IsWidgetCreated)
			el.ApplyAllowDrop(el._widget!, (bool)(e.NewValue ?? false));
	}

	/// <summary>
	/// Gets or sets whether this element can be a drop target.
	/// When set to <see langword="true"/>, a <c>Gtk.DropTarget</c> controller is attached
	/// to the backing widget (if one exists and a display is available).
	/// </summary>
	public bool AllowDrop
	{
		get => (bool)(GetValue(AllowDropProperty) ?? false);
		set => SetValue(AllowDropProperty, value);
	}

	// ── Routed events — DragEnter ─────────────────────────────────────────

	/// <summary>Tunnel PreviewDragEnter.</summary>
	public static readonly RoutedEvent PreviewDragEnterEvent =
		EventManager.RegisterRoutedEvent(
			"PreviewDragEnter", RoutingStrategy.Tunnel,
			typeof(DragEventHandler), typeof(UIElement));

	/// <summary>Bubble DragEnter.</summary>
	public static readonly RoutedEvent DragEnterEvent =
		EventManager.RegisterRoutedEvent(
			"DragEnter", RoutingStrategy.Bubble,
			typeof(DragEventHandler), typeof(UIElement));

	/// <summary>Occurs when a drag operation enters this element (tunnel).</summary>
	public event DragEventHandler? PreviewDragEnter
	{
		add    => AddHandler(PreviewDragEnterEvent, value!);
		remove => RemoveHandler(PreviewDragEnterEvent, value!);
	}

	/// <summary>Occurs when a drag operation enters this element (bubble).</summary>
	public event DragEventHandler? DragEnter
	{
		add    => AddHandler(DragEnterEvent, value!);
		remove => RemoveHandler(DragEnterEvent, value!);
	}

	// ── Routed events — DragOver ──────────────────────────────────────────

	/// <summary>Tunnel PreviewDragOver.</summary>
	public static readonly RoutedEvent PreviewDragOverEvent =
		EventManager.RegisterRoutedEvent(
			"PreviewDragOver", RoutingStrategy.Tunnel,
			typeof(DragEventHandler), typeof(UIElement));

	/// <summary>Bubble DragOver.</summary>
	public static readonly RoutedEvent DragOverEvent =
		EventManager.RegisterRoutedEvent(
			"DragOver", RoutingStrategy.Bubble,
			typeof(DragEventHandler), typeof(UIElement));

	/// <summary>Occurs while a drag operation is over this element (tunnel).</summary>
	public event DragEventHandler? PreviewDragOver
	{
		add    => AddHandler(PreviewDragOverEvent, value!);
		remove => RemoveHandler(PreviewDragOverEvent, value!);
	}

	/// <summary>Occurs while a drag operation is over this element (bubble).</summary>
	public event DragEventHandler? DragOver
	{
		add    => AddHandler(DragOverEvent, value!);
		remove => RemoveHandler(DragOverEvent, value!);
	}

	// ── Routed events — DragLeave ─────────────────────────────────────────

	/// <summary>Tunnel PreviewDragLeave.</summary>
	public static readonly RoutedEvent PreviewDragLeaveEvent =
		EventManager.RegisterRoutedEvent(
			"PreviewDragLeave", RoutingStrategy.Tunnel,
			typeof(DragEventHandler), typeof(UIElement));

	/// <summary>Bubble DragLeave.</summary>
	public static readonly RoutedEvent DragLeaveEvent =
		EventManager.RegisterRoutedEvent(
			"DragLeave", RoutingStrategy.Bubble,
			typeof(DragEventHandler), typeof(UIElement));

	/// <summary>Occurs when a drag operation leaves this element (tunnel).</summary>
	public event DragEventHandler? PreviewDragLeave
	{
		add    => AddHandler(PreviewDragLeaveEvent, value!);
		remove => RemoveHandler(PreviewDragLeaveEvent, value!);
	}

	/// <summary>Occurs when a drag operation leaves this element (bubble).</summary>
	public event DragEventHandler? DragLeave
	{
		add    => AddHandler(DragLeaveEvent, value!);
		remove => RemoveHandler(DragLeaveEvent, value!);
	}

	// ── Routed events — Drop ──────────────────────────────────────────────

	/// <summary>Tunnel PreviewDrop.</summary>
	public static readonly RoutedEvent PreviewDropEvent =
		EventManager.RegisterRoutedEvent(
			"PreviewDrop", RoutingStrategy.Tunnel,
			typeof(DragEventHandler), typeof(UIElement));

	/// <summary>Bubble Drop.</summary>
	public static readonly RoutedEvent DropEvent =
		EventManager.RegisterRoutedEvent(
			"Drop", RoutingStrategy.Bubble,
			typeof(DragEventHandler), typeof(UIElement));

	/// <summary>Occurs when data is dropped onto this element (tunnel).</summary>
	public event DragEventHandler? PreviewDrop
	{
		add    => AddHandler(PreviewDropEvent, value!);
		remove => RemoveHandler(PreviewDropEvent, value!);
	}

	/// <summary>Occurs when data is dropped onto this element (bubble).</summary>
	public event DragEventHandler? Drop
	{
		add    => AddHandler(DropEvent, value!);
		remove => RemoveHandler(DropEvent, value!);
	}

	// ── Partial GTK hook (implemented in UIElement.GtkDragDrop.cs) ───────────

	/// <summary>
	/// Applies or removes the <c>Gtk.DropTarget</c> controller for <see cref="AllowDrop"/>.
	/// Implemented in <c>UIElement.GtkDragDrop.cs</c>.
	/// </summary>
	partial void ApplyAllowDrop(Gtk.Widget widget, bool allow);

	/// <summary>
	/// Called from <c>UIElement.GtkInput.cs</c> after the backing widget is created;
	/// installs a <c>Gtk.DropTarget</c> if <see cref="AllowDrop"/> is already set.
	/// Implemented in <c>UIElement.GtkDragDrop.cs</c>.
	/// </summary>
	internal partial void AttachDragDropOnCreated(Gtk.Widget widget);
}
