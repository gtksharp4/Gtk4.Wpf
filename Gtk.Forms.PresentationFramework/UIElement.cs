using System;
using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Base class for all WPF visual elements. Each UIElement owns a backing
/// <see cref="Gtk.Widget"/> created on first access via <see cref="CreateWidget"/>.
/// </summary>
public abstract partial class UIElement : DependencyObject
{
	Gtk.Widget? _widget;

	/// <summary>
	/// Creates the backing GTK widget. Called once; result is cached in <see cref="Widget"/>.
	/// </summary>
	protected abstract Gtk.Widget CreateWidget();

	/// <summary>
	/// The backing GTK widget for this element. Created lazily on first access.
	/// </summary>
	public Gtk.Widget Widget
	{
		get
		{
			if (_widget != null) return _widget;
			_widget = CreateWidget();
			OnWidgetCreated(_widget);
			return _widget;
		}
	}

	/// <summary>
	/// Returns <c>true</c> when <see cref="Widget"/> has been materialised.
	/// Safe to call without forcing widget creation.
	/// </summary>
	internal virtual bool IsWidgetCreated => _widget != null;

	/// <summary>
	/// Called once immediately after <see cref="Widget"/> is first created.
	/// Override to apply deferred state (e.g. <c>TooltipText</c>) to the new widget.
	/// </summary>
	protected virtual void OnWidgetCreated(Gtk.Widget widget)
	{
		// U2: apply any Opacity set before widget creation.
		var opacity = (double)(GetValue(OpacityProperty) ?? 1.0);
		if (opacity != 1.0)
			widget.Opacity = opacity;

		// Hook: attach GTK input controllers (focus, key, mouse, motion).
		// Implemented in UIElement.GtkInput.cs as a partial method call.
		AttachInputControllersOnCreated(widget);
	}

	/// <summary>
	/// Called from <see cref="OnWidgetCreated"/> to attach GTK input controllers.
	/// Implemented in <c>UIElement.GtkInput.cs</c>.
	/// </summary>
	partial void AttachInputControllersOnCreated(Gtk.Widget widget);

	// ── Opacity ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// Opacity DP: double in [0,1], default 1.0.
	/// Applied to the backing <see cref="Gtk.Widget"/> immediately when the widget exists;
	/// deferred to <see cref="OnWidgetCreated"/> otherwise (lazy-safe).
	/// </summary>
	public static readonly DependencyProperty OpacityProperty =
		DependencyProperty.Register(
			nameof(Opacity),
			typeof(double),
			typeof(UIElement),
			new PropertyMetadata(1.0, OnOpacityChanged));

	static void OnOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is UIElement el && el.IsWidgetCreated)
			el._widget!.Opacity = (double)(e.NewValue ?? 1.0);
	}

	/// <summary>Gets or sets the opacity (0.0 = fully transparent, 1.0 = fully opaque).</summary>
	public double Opacity
	{
		get => (double)(GetValue(OpacityProperty) ?? 1.0);
		set => SetValue(OpacityProperty, value);
	}

	// ── Measure / Arrange — see UIElement.Layout.cs ──────────────────────────

	/// <summary>The size this element measured to on the last <see cref="Measure"/> call.</summary>
	public Size DesiredSize { get; protected set; }

	// ── Visual children (for TemplatedParent subtree stamping) ──────────────────

	/// <summary>
	/// Returns the direct visual children of this element.
	/// Override in <see cref="Controls.Panel"/> and content controls to expose the actual
	/// child collection. The base implementation returns an empty enumerable.
	/// Used by <see cref="Controls.Control"/> to stamp <c>TemplatedParent</c> on the
	/// entire produced subtree after a template is applied.
	/// </summary>
	internal virtual IEnumerable<UIElement> VisualChildren =>
		System.Linq.Enumerable.Empty<UIElement>();

	// ── Logical parent ───────────────────────────────────────────────────────────

	UIElement? _logicalParent;

	/// <summary>
	/// Logical parent; setting this also updates the DP inheritance parent so
	/// inheriting properties (e.g. DataContext) propagate automatically.
	/// </summary>
	internal UIElement? LogicalParent
	{
		get => _logicalParent;
		set
		{
			if (ReferenceEquals(_logicalParent, value)) return;
			_logicalParent = value;
			// Wire DP inheritance through the same parent.
			InheritanceParent = value;
		}
	}

	// ── Routed events ─────────────────────────────────────────────────────────

	/// <summary>
	/// Adds a handler for a routed event. Phase-0: handler stored in a simple list;
	/// routing walks the logical parent chain.
	/// </summary>
	public void AddHandler(RoutedEvent routedEvent, Delegate handler)
	{
		if (routedEvent == null) throw new ArgumentNullException(nameof(routedEvent));
		if (handler == null) throw new ArgumentNullException(nameof(handler));
		RoutedEventHost.AddHandler(this, routedEvent, handler);
	}

	/// <summary>
	/// Adds a handler, optionally receiving already-handled events.
	/// WPF-compatible 3-argument overload; unmodified WPF code uses this form.
	/// </summary>
	public void AddHandler(RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
	{
		if (routedEvent == null) throw new ArgumentNullException(nameof(routedEvent));
		if (handler == null) throw new ArgumentNullException(nameof(handler));
		RoutedEventHost.AddHandler(this, routedEvent, handler, handledEventsToo);
	}

	/// <summary>Removes a previously added routed-event handler.</summary>
	public void RemoveHandler(RoutedEvent routedEvent, Delegate handler)
	{
		if (routedEvent == null) throw new ArgumentNullException(nameof(routedEvent));
		if (handler == null) throw new ArgumentNullException(nameof(handler));
		RoutedEventHost.RemoveHandler(this, routedEvent, handler);
	}

	/// <summary>
	/// Raises <paramref name="e"/> on this element, routing it according to its strategy.
	/// </summary>
	protected void RaiseEvent(RoutedEventArgs e)
	{
		if (e == null) throw new ArgumentNullException(nameof(e));
		RoutedEventHost.RaiseEvent(this, e);
	}
}