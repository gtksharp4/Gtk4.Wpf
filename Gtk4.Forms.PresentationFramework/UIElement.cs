using System;
using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Base class for all WPF visual elements. Each UIElement owns a backing
/// <see cref="Gtk.Widget"/> created on first access via <see cref="CreateWidget"/>.
/// </summary>
public abstract partial class UIElement : Media.Visual
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

		// Apply any RenderTransform set before widget creation.
		// Implemented in UIElement.RenderTransform.cs.
		ApplyRenderTransformOnCreated(widget);

		// Apply any Effect set before widget creation.
		// Implemented in UIElement.Effect.cs.
		ApplyEffectOnCreated(widget);

		// Apply Background/Foreground/Font*/Border* CSS set before widget creation.
		// Implemented in UIElement.Appearance.cs.
		ApplyAppearanceOnCreated(widget);

		// Apply IsTabStop/Focusable → widget.CanFocus.
		// Implemented in UIElement.TabNavigation.cs.
		ApplyTabNavigationOnCreated(widget);

		// Push AutomationProperties.Name / HelpText → GTK accessible properties.
		// Implemented in UIElement.Automation.cs.
		ApplyAutomationOnCreated(widget);
	}

	/// <summary>
	/// Called from <see cref="OnWidgetCreated"/> to attach GTK input controllers.
	/// Implemented in <c>UIElement.GtkInput.cs</c>.
	/// </summary>
	partial void AttachInputControllersOnCreated(Gtk.Widget widget);

	/// <summary>
	/// Called from <see cref="OnWidgetCreated"/> to apply any
	/// <see cref="RenderTransform"/> set before widget creation.
	/// Implemented in <c>UIElement.RenderTransform.cs</c>.
	/// </summary>
	partial void ApplyRenderTransformOnCreated(Gtk.Widget widget);

	/// <summary>
	/// Called from <see cref="OnWidgetCreated"/> to apply any
	/// <see cref="Effect"/> set before widget creation.
	/// Implemented in <c>UIElement.Effect.cs</c>.
	/// </summary>
	partial void ApplyEffectOnCreated(Gtk.Widget widget);

	/// <summary>Called from <see cref="OnWidgetCreated"/>; implemented in <c>UIElement.Appearance.cs</c>.</summary>
	partial void ApplyAppearanceOnCreated(Gtk.Widget widget);

	/// <summary>
	/// Called from <see cref="OnWidgetCreated"/> to apply <see cref="IsTabStop"/> and
	/// <see cref="Focusable"/> → <c>widget.CanFocus</c>.
	/// Implemented in <c>UIElement.TabNavigation.cs</c>.
	/// </summary>
	partial void ApplyTabNavigationOnCreated(Gtk.Widget widget);

	/// <summary>
	/// Called from <see cref="OnWidgetCreated"/> to push any
	/// <see cref="Automation.AutomationProperties"/> values set before widget creation
	/// to the GTK 4 accessible API.
	/// Implemented in <c>UIElement.Automation.cs</c>.
	/// </summary>
	partial void ApplyAutomationOnCreated(Gtk.Widget widget);

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

	/// <inheritdoc/>
	protected internal override int VisualChildrenCount
	{
		get
		{
			int count = 0;
			foreach (var _ in VisualChildren) count++;
			return count;
		}
	}

	/// <inheritdoc/>
	protected internal override Media.Visual GetVisualChild(int index)
	{
		int i = 0;
		foreach (var child in VisualChildren)
		{
			if (i == index) return child;
			i++;
		}
		throw new ArgumentOutOfRangeException(nameof(index));
	}

	// ── Logical parent ───────────────────────────────────────────────────────────

	UIElement? _logicalParent;

	/// <summary>
	/// Logical parent; setting this also updates the DP inheritance parent so
	/// inheriting properties (e.g. DataContext) propagate automatically.
	/// Also updates <see cref="Media.Visual.VisualParent"/> so <see cref="Media.VisualTreeHelper"/> can walk up.
	/// </summary>
	internal UIElement? LogicalParent
	{
		get => _logicalParent;
		set
		{
			if (ReferenceEquals(_logicalParent, value)) return;
			var oldParent = _logicalParent;
			_logicalParent = value;
			// Keep Visual.VisualParent in sync.
			VisualParent = value;
			// Wire DP inheritance through the same parent.
			InheritanceParent = value;
			// Lazy Initialized + deferred subtree Loaded/Unloaded (UIElement.Lifecycle.cs).
			OnLogicalParentChangedLifecycle(oldParent, value);
		}
	}

	/// <summary>
	/// Called from the <see cref="LogicalParent"/> setter after the parent changed.
	/// Implemented in <c>UIElement.Lifecycle.cs</c>.
	/// </summary>
	partial void OnLogicalParentChangedLifecycle(UIElement? oldParent, UIElement? newParent);
}