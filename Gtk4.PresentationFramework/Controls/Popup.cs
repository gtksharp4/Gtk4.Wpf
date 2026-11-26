namespace System.Windows.Controls.Primitives;

/// <summary>
/// A floating content host backed by <see cref="Gtk.Popover"/>.
/// <para>
/// Two-way: setting <see cref="IsOpen"/><c> = true</c> calls <c>Popover.Popup()</c>;
/// the GTK <c>Closed</c> signal writes back <c>IsOpen = false</c>.
/// </para>
/// <para>
/// <see cref="PlacementTarget"/> must be set to a realized <see cref="UIElement"/>
/// before opening; the popover anchors to its backing <see cref="Gtk.Widget"/>.
/// When no target is set the popover is parented to its own widget (floats).
/// </para>
/// <para>
/// <see cref="StaysOpen"/><c> = false</c>: the GTK <c>autohide</c> property is set,
/// so the popover closes when focus moves away (GTK4 default). When <c>true</c> the
/// popover only closes on explicit <c>IsOpen = false</c>.
/// </para>
/// </summary>
public partial class Popup : FrameworkElement
{
	// ── IsOpen DP ─────────────────────────────────────────────────────────────

	/// <summary><c>true</c> while the popup is visible.</summary>
	public static readonly DependencyProperty IsOpenProperty =
		DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(Popup),
			new FrameworkPropertyMetadata(false,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));

	static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var p = (Popup)d;
		if (p._updating) return;
		var open = (bool?)e.NewValue == true;
		// Widget not yet materialised: remember intent; replay at OnWidgetCreated.
		if (!p.IsWidgetCreated)
		{
			p._pendingOpen = open;
			return;
		}
		if (open) p.DoOpen(); else p.DoClose();
	}

	/// <summary>Gets or sets whether the popup is open (two-way with GTK).</summary>
	public bool IsOpen
	{
		get => (bool?)GetValue(IsOpenProperty) == true;
		set => SetValue(IsOpenProperty, value);
	}

	// ── Child DP ─────────────────────────────────────────────────────────────

	/// <summary>The content hosted inside the popup.</summary>
	public static readonly DependencyProperty ChildProperty =
		DependencyProperty.Register(nameof(Child), typeof(UIElement), typeof(Popup),
			new PropertyMetadata(null, OnChildChanged));

	static void OnChildChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var p = (Popup)d;
		if (p._popover == null) return;
		p._popover.Child = (e.NewValue as UIElement)?.Widget;
	}

	/// <summary>Gets or sets the child UIElement displayed in the popup.</summary>
	public UIElement? Child
	{
		get => (UIElement?)GetValue(ChildProperty);
		set => SetValue(ChildProperty, value);
	}

	// ── PlacementTarget DP ────────────────────────────────────────────────────

	/// <summary>The element the popup anchors to.</summary>
	public static readonly DependencyProperty PlacementTargetProperty =
		DependencyProperty.Register(nameof(PlacementTarget), typeof(UIElement), typeof(Popup),
			new PropertyMetadata(null));

	/// <summary>Gets or sets the placement target.</summary>
	public UIElement? PlacementTarget
	{
		get => (UIElement?)GetValue(PlacementTargetProperty);
		set => SetValue(PlacementTargetProperty, value);
	}

	// ── StaysOpen DP ──────────────────────────────────────────────────────────

	/// <summary>
	/// When <c>false</c> the popup auto-closes on focus loss (GTK4 autohide).
	/// When <c>true</c> it only closes when <see cref="IsOpen"/> is set to <c>false</c>.
	/// </summary>
	public static readonly DependencyProperty StaysOpenProperty =
		DependencyProperty.Register(nameof(StaysOpen), typeof(bool), typeof(Popup),
			new PropertyMetadata(true));

	/// <summary>Gets or sets whether the popup stays open without explicit close.</summary>
	public bool StaysOpen
	{
		get => (bool?)GetValue(StaysOpenProperty) != false;
		set => SetValue(StaysOpenProperty, value);
	}

	// ── GTK backing ───────────────────────────────────────────────────────────

	Gtk.Popover? _popover;
	bool _updating;
	/// <summary>
	/// Remembers an <see cref="IsOpen"/> assignment made before the backing widget was
	/// created. Consumed and cleared in <see cref="OnWidgetCreated"/>.
	/// </summary>
	internal bool _pendingOpen;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_popover = new Gtk.Popover();
		// autohide = !StaysOpen
		_popover.Autohide = !StaysOpen;
		if (Child != null)
			_popover.Child = Child.Widget;

		// Anchor to PlacementTarget if available; otherwise float on itself.
		var anchor = PlacementTarget?.Widget ?? _popover;
		if (!ReferenceEquals(anchor, _popover))
			_popover.Parent = anchor;

		ApplyPlacement(_popover);
		_popover.Closed += OnPopoverClosed;
		return _popover;
	}

	void DoOpen()
	{
		if (_popover == null) _ = Widget; // force creation
		_popover!.Autohide = !StaysOpen;
		var anchor = PlacementTarget?.Widget;
		if (anchor != null) _popover.Parent = anchor;
		ApplyPlacement(_popover);
		_popover.Popup();
	}

	void DoClose()
	{
		_popover?.Popdown();
	}

	void OnPopoverClosed(object? sender, EventArgs e)
	{
		_updating = true;
		try { SetValue(IsOpenProperty, false); }
		finally { _updating = false; }
	}

	/// <inheritdoc/>
	protected override void OnWidgetCreated(Gtk.Widget widget)
	{
		base.OnWidgetCreated(widget);
		// Replay any IsOpen assignment that arrived before the backing widget existed.
		if (_pendingOpen)
		{
			_pendingOpen = false;
			DoOpen();
		}
	}

	/// <summary>Returns the underlying <see cref="Gtk.Popover"/> (for GTK smoke tests).</summary>
	internal Gtk.Popover? GtkPopover => _popover;
}