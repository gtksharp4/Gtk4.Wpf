namespace System.Windows.Controls;

/// <summary>Specifies the side to which a child is docked in a <see cref="DockPanel"/>.</summary>
public enum Dock
{
	/// <summary>Docked to the left edge.</summary>
	Left = 0,
	/// <summary>Docked to the top edge.</summary>
	Top = 1,
	/// <summary>Docked to the right edge.</summary>
	Right = 2,
	/// <summary>Docked to the bottom edge.</summary>
	Bottom = 3
}

/// <summary>
/// Arranges children docked to the four sides, with the last child optionally
/// filling remaining space (<see cref="LastChildFill"/>).
///
/// GTK backing: a nested <see cref="Gtk.Box"/> arrangement.
/// Top/Bottom children go into the outer vertical box; Left/Right children go into an inner
/// horizontal box in the centre slot. Span / overlap fidelity matches GTK's box model, not
/// WPF's exact pixel-precise dock algorithm — noted approximation.
/// </summary>
public partial class DockPanel : Panel
{
	// ── Attached property: Dock ──────────────────────────────────────────────────

	/// <summary>Attached property that controls which side a child docks to.</summary>
	public static readonly DependencyProperty DockProperty =
		DependencyProperty.RegisterAttached(
			"Dock", typeof(Dock), typeof(DockPanel),
			new PropertyMetadata(Dock.Left, OnDockChanged));

	/// <summary>Gets the dock side for <paramref name="element"/>.</summary>
	public static Dock GetDock(UIElement element) => (Dock)element.GetValue(DockProperty)!;

	/// <summary>Sets the dock side for <paramref name="element"/>.</summary>
	public static void SetDock(UIElement element, Dock value) => element.SetValue(DockProperty, value);

	static void OnDockChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is UIElement child && child.LogicalParent is DockPanel dp && dp._outerBox != null)
			dp.RebuildLayout();
	}

	// ── LastChildFill ────────────────────────────────────────────────────────────

	/// <summary>
	/// When <c>true</c> (default), the last child fills remaining space rather than
	/// docking to a specific side.
	/// </summary>
	public static readonly DependencyProperty LastChildFillProperty =
		DependencyProperty.Register(
			nameof(LastChildFill), typeof(bool), typeof(DockPanel),
			new PropertyMetadata(true, OnLastChildFillChanged));

	static void OnLastChildFillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is DockPanel dp && dp._outerBox != null)
			dp.RebuildLayout();
	}

	/// <summary>Gets or sets whether the last child fills the remaining space.</summary>
	public bool LastChildFill
	{
		get => (bool)GetValue(LastChildFillProperty)!;
		set => SetValue(LastChildFillProperty, value);
	}

	// ── GTK backing ──────────────────────────────────────────────────────────────

	// Outer vertical box: top→[inner hbox]→bottom
	Gtk.Box? _outerBox;
	// Inner horizontal box: left→[fill]→right
	Gtk.Box? _innerBox;
	// Placeholder filler widget (expands to fill the centre slot)
	Gtk.Box? _fillSlot;

	/// <inheritdoc/>
	internal override bool IsWidgetCreated => _outerBox != null;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_outerBox = new BackdropBox(Gtk.Orientation.Vertical, 0);
		_innerBox = new Gtk.Box(Gtk.Orientation.Horizontal, 0);
		_fillSlot = new Gtk.Box(Gtk.Orientation.Vertical, 0);
		_fillSlot.Hexpand = true;
		_fillSlot.Vexpand = true;
		_innerBox.Add(_fillSlot);
		_outerBox.Add(_innerBox);
		_innerBox.Hexpand = true;
		_innerBox.Vexpand = true;
		BuildLayout();
		return _outerBox;
	}

	/// <inheritdoc/>
	protected override void AddChildWidget(UIElement child)
	{
		RebuildLayout();
	}

	/// <inheritdoc/>
	protected override void RemoveChildWidget(UIElement child)
	{
		// Remove from wherever it currently sits; then rebuild.
		SafeRemove(child.Widget);
		RebuildLayout();
	}

	void SafeRemove(Gtk.Widget w)
	{
		try { _outerBox?.Remove(w); } catch { }
		try { _innerBox?.Remove(w); } catch { }
		try { _fillSlot?.Remove(w); } catch { }
	}

	void RebuildLayout()
	{
		if (_outerBox == null) return;

		// Detach all children from all slots.
		foreach (var child in Children)
			SafeRemove(child.Widget);

		BuildLayout();
	}

	void BuildLayout()
	{
		int count = Children.Count;
		for (int i = 0; i < count; i++)
		{
			var child = Children[i];
			bool isFill = LastChildFill && i == count - 1;

			if (isFill)
			{
				child.Widget.Hexpand = true;
				child.Widget.Vexpand = true;
				_fillSlot!.Add(child.Widget);
			}
			else
			{
				var dock = GetDock(child);
				switch (dock)
				{
					case Dock.Top:
						child.Widget.Hexpand = true;
						_outerBox!.Prepend(child.Widget);
						break;
					case Dock.Bottom:
						child.Widget.Hexpand = true;
						_outerBox!.Append(child.Widget);
						break;
					case Dock.Left:
						child.Widget.Vexpand = true;
						_innerBox!.Prepend(child.Widget);
						break;
					case Dock.Right:
						child.Widget.Vexpand = true;
						_innerBox!.Append(child.Widget);
						break;
				}
			}
		}
	}
}