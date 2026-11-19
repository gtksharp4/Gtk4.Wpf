using System.Collections.Generic;
using System.Windows.Input;

namespace System.Windows;

/// <summary>
/// <see cref="Visibility"/>: Collapsed hides the GTK widget (containers skip it in measure and
/// allocate, like WPF Collapsed); Hidden keeps it mapped but at opacity 0, untargetable and
/// unfocusable, like WPF's Hidden. Mapping is in <see cref="VisibilityGtkMap"/>.
/// </summary>
/// <remarks>
/// Gap: WPF's invalid-enum <c>ValidateValueCallback</c> (ArgumentException) is not applied; the
/// property engine has no validate hook yet.
/// </remarks>
public abstract partial class UIElement
{
	/// <summary>Identifies the <see cref="Visibility"/> dependency property.</summary>
	public static readonly DependencyProperty VisibilityProperty =
		DependencyProperty.Register(nameof(Visibility), typeof(Visibility), typeof(UIElement),
			new PropertyMetadata(Visibility.Visible, OnVisibilityChanged),
			v => v is Visibility vis && vis >= Visibility.Visible && vis <= Visibility.Collapsed);

	/// <summary>Gets or sets the display state of this element.</summary>
	public Visibility Visibility
	{
		get => (Visibility)(GetValue(VisibilityProperty) ?? Visibility.Visible);
		set => SetValue(VisibilityProperty, value);
	}

	// Set when this layer hid the widget for Collapsed, so only that hide is undone.
	bool _widgetCollapsed;

	// Shared with Window, which overrides the metadata default to Collapsed.
	internal static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var el = (UIElement)d;
		var old = (Visibility)(e.OldValue ?? Visibility.Visible);
		var now = (Visibility)(e.NewValue ?? Visibility.Visible);
		if ((old == Visibility.Collapsed) != (now == Visibility.Collapsed))
		{
			el.InvalidateMeasure();
			el.LogicalParent?.InvalidateMeasure();
		}
		el.OnVisibilityChangedCore(now);
		if (el.HasBackingWidget)
			el.ApplyVisibilityToWidget(el._widget!);
		el.RecomputeIsVisible();
		if (now != Visibility.Visible)
			el.ReleaseInputWithin();
	}

	/// <summary>The visibility the GTK widget mapping uses; a top-level overrides it.</summary>
	internal virtual Visibility WidgetVisibility => Visibility;

	/// <summary>Hook for subclasses that own the GTK window state (<see cref="Window"/>).</summary>
	internal virtual void OnVisibilityChangedCore(Visibility visibility) { }

	/// <summary>Pushes Visibility to the widget: Visible flag, opacity, CanTarget and CanFocus.</summary>
	internal virtual void ApplyVisibilityToWidget(Gtk.Widget widget)
	{
		var flags = VisibilityGtkMap.Map(WidgetVisibility);
		if (!flags.WidgetVisible)
		{
			widget.Visible = false;
			_widgetCollapsed = true;
		}
		else if (_widgetCollapsed)
		{
			widget.Visible = true;
			_widgetCollapsed = false;
		}
		widget.Opacity = VisibilityGtkMap.EffectiveOpacity(WidgetVisibility, Opacity);
		ApplyCanTarget(widget);
		ApplyCanFocus(FocusWidget);
	}

	partial void ApplyVisibilityStateOnCreated(Gtk.Widget widget)
	{
		if (WidgetVisibility != Visibility.Visible)
			ApplyVisibilityToWidget(widget);
		else if (Opacity != 1.0)
			widget.Opacity = Opacity;
		if (!IsEnabled)
			widget.Sensitive = false;
		if (!IsHitTestVisible)
			ApplyCanTarget(widget);
	}

	/// <summary>
	/// <c>true</c> when this element and its ancestors are all <see cref="Visibility.Visible"/>.
	/// A <see cref="Window"/> ends the walk: its own Visibility is the show state, which is the
	/// presentation-source half of <see cref="IsVisible"/> that Focus/capture gates deliberately skip
	/// so an unshown (headless) tree stays focusable and capturable.
	/// </summary>
	internal bool IsVisibilityChainVisible
	{
		get
		{
			for (UIElement? e = this; e != null && !(e is Window); e = e.LogicalParent)
				if (e.Visibility != Visibility.Visible) return false;
			return true;
		}
	}

	/// <summary>Drops keyboard focus and mouse capture held by this element or a descendant.</summary>
	void ReleaseInputWithin()
	{
		if (Keyboard.FocusedElement is UIElement focused && IsSelfOrAncestorOf(focused))
			focused.SetFocusedInternal(false);
		if (Mouse.Captured is UIElement captor && IsSelfOrAncestorOf(captor))
			Mouse.Capture(null);
	}

	bool IsSelfOrAncestorOf(UIElement other)
	{
		for (UIElement? e = other; e != null; e = e.LogicalParent)
			if (ReferenceEquals(e, this)) return true;
		return false;
	}

	/// <summary>Snapshot of the logical children, safe to iterate while handlers re-parent.</summary>
	List<UIElement> ChildSnapshot() => new List<UIElement>(LifecycleChildren);
}
