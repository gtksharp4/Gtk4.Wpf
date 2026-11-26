using System;
using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>
/// A single-child decorator that places an <see cref="AdornerLayer"/> above its
/// child, so adorners registered with <see cref="AdornerLayer.GetAdornerLayer(Media.Visual)"/>
/// resolve to this layer.
/// </summary>
/// <remarks>
/// GTK rendering: backed by a <see cref="Gtk.Overlay"/> with the child widget as
/// the main content and adorner widgets added as overlay children.
/// Object model is fully headless-safe; overlay wiring happens only when
/// <see cref="UIElement.CreateWidget"/> is called.
/// </remarks>
[ContentProperty("Child")]
public class AdornerDecorator : FrameworkElement
{
	/// <summary>The adorner layer belonging to this decorator.</summary>
	public AdornerLayer AdornerLayer { get; } = new AdornerLayer();

	// ── Child DP ─────────────────────────────────────────────────────────────

	/// <summary>The single child element.</summary>
	public static readonly DependencyProperty ChildProperty =
		DependencyProperty.Register(
			nameof(Child),
			typeof(UIElement),
			typeof(AdornerDecorator),
			new PropertyMetadata(null, OnChildChanged));

	static void OnChildChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var dec = (AdornerDecorator)d;
		if (e.OldValue is UIElement old)
		{
			old.LogicalParent = null;
			if (dec._overlay != null)
				dec._overlay.Child = null;
		}
		if (e.NewValue is UIElement newEl)
		{
			newEl.LogicalParent = dec;
			if (dec._overlay != null)
				dec._overlay.Child = newEl.Widget;
		}
	}

	/// <summary>Gets or sets the child element.</summary>
	public UIElement? Child
	{
		get => (UIElement?)GetValue(ChildProperty);
		set => SetValue(ChildProperty, value);
	}

	// ── GTK backing ──────────────────────────────────────────────────────────

	Gtk.Overlay? _overlay;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_overlay = new Gtk.Overlay();
		AdornerLayer.GtkOverlay = _overlay;
		AdornerLayer.HookFrameLayout(_overlay);
		if (Child != null)
			_overlay.Child = Child.Widget;
		return _overlay;
	}

	// ── Visual tree ──────────────────────────────────────────────────────────

	/// <inheritdoc/>
	internal override IEnumerable<UIElement> VisualChildren =>
		Child != null ? new[] { Child } : System.Linq.Enumerable.Empty<UIElement>();

	// ── Layout ───────────────────────────────────────────────────────────────

	/// <inheritdoc/>
	protected override Size MeasureOverride(Size availableSize)
	{
		if (Child == null) return default;
		Child.Measure(availableSize);
		return Child.DesiredSize;
	}

	/// <inheritdoc/>
	protected override Size ArrangeOverride(Size finalSize)
	{
		Child?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
		return finalSize;
	}
}
