namespace System.Windows;

/// <summary>
/// <see cref="IsTabStop"/> and <see cref="TabIndex"/> dependency properties on
/// <see cref="UIElement"/>.
/// <para>
/// <b>GTK application:</b> <see cref="IsTabStop"/> and <see cref="Focusable"/> together
/// control <c>Gtk.Widget.CanFocus</c> (<c>widget.CanFocus = IsTabStop &amp;&amp; Focusable</c>)
/// when the backing widget is realized. <see cref="TabIndex"/> has no direct GTK per-widget
/// ordering API — GTK 4 sequences focusable widgets by visual tree order. The TabIndex value
/// is stored in the DP system and exposed for managed tab-order computation via
/// <see cref="Input.TabOrderHelper"/>.
/// </para>
/// </summary>
public abstract partial class UIElement
{
	// ── IsTabStop DP ─────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="IsTabStop"/> dependency property.
	/// Default: <c>true</c>.
	/// </summary>
	public static readonly DependencyProperty IsTabStopProperty =
		DependencyProperty.Register(
			nameof(IsTabStop),
			typeof(bool),
			typeof(UIElement),
			new PropertyMetadata(true, OnTabFocusabilityChanged));

	/// <summary>
	/// Gets or sets whether this element is included in Tab-key navigation.
	/// When <c>false</c>, <c>Gtk.Widget.CanFocus</c> is set to <c>false</c> on the
	/// backing widget regardless of <see cref="Focusable"/>.
	/// </summary>
	public bool IsTabStop
	{
		get => (bool)(GetValue(IsTabStopProperty) ?? true);
		set => SetValue(IsTabStopProperty, value);
	}

	// ── TabIndex DP ───────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="TabIndex"/> dependency property.
	/// Default: <see cref="int.MaxValue"/> (matches WPF default; elements with the same
	/// TabIndex are ordered by declaration order).
	/// </summary>
	/// <remarks>
	/// GTK 4 does not expose a per-widget tab-order integer; this value is used only by
	/// the managed <see cref="Input.TabOrderHelper"/> and has no effect on the native
	/// GTK tab sequence.
	/// </remarks>
	public static readonly DependencyProperty TabIndexProperty =
		DependencyProperty.Register(
			nameof(TabIndex),
			typeof(int),
			typeof(UIElement),
			new PropertyMetadata(int.MaxValue));

	/// <summary>
	/// Gets or sets the tab-navigation order for this element within its container.
	/// Lower values are visited first; ties are resolved by declaration order.
	/// </summary>
	/// <remarks>
	/// GTK 4 limitation: this value is not propagated to the native widget;
	/// GTK sequences focusable widgets by visual tree order. Use
	/// <see cref="Input.TabOrderHelper"/> to apply WPF tab order in managed code.
	/// </remarks>
	public int TabIndex
	{
		get => (int)(GetValue(TabIndexProperty) ?? int.MaxValue);
		set => SetValue(TabIndexProperty, value);
	}

	// ── GTK application ───────────────────────────────────────────────────────

	static void OnTabFocusabilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is UIElement el && el.IsWidgetCreated)
			el.ApplyCanFocus(el.FocusWidget);
	}

	/// <summary>
	/// Applies <c>widget.CanFocus = IsTabStop &amp;&amp; Focusable</c> to the focus widget.
	/// Called on widget creation and when either DP changes.
	/// </summary>
	void ApplyCanFocus(Gtk.Widget widget)
	{
		widget.CanFocus = VisibilityGtkMap.CanFocus(WidgetVisibility, IsTabStop && Focusable);
	}

	/// <summary>
	/// Called from <see cref="OnWidgetCreated"/> to apply tab-focusability state
	/// that was set before widget creation.
	/// Implemented in this file; wired via partial method in <c>UIElement.cs</c>.
	/// </summary>
	partial void ApplyTabNavigationOnCreated(Gtk.Widget widget) =>
		// Use FocusWidget (the inner focusable widget — e.g. a multiline TextBox's TextView),
		// matching the change-callback path; the raw created widget may be an outer wrapper.
		ApplyCanFocus(FocusWidget);
}
