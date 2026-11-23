namespace System.Windows;

public partial class FrameworkElement
{
	// ── ContextMenu attached DP ───────────────────────────────────────────────

	/// <summary>Attached DP: the context menu for any <see cref="FrameworkElement"/>.</summary>
	public static readonly DependencyProperty ContextMenuProperty =
		DependencyProperty.RegisterAttached(
			"ContextMenu",
			typeof(Controls.ContextMenu),
			typeof(FrameworkElement),
			new PropertyMetadata(null));

	/// <summary>Gets the context menu attached to <paramref name="element"/>.</summary>
	public static Controls.ContextMenu? GetContextMenu(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (Controls.ContextMenu?)element.GetValue(ContextMenuProperty);
	}

	/// <summary>Sets the context menu on <paramref name="element"/>.</summary>
	public static void SetContextMenu(DependencyObject element, Controls.ContextMenu? value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(ContextMenuProperty, value);
	}

	/// <summary>Instance accessor for the context menu on this element.</summary>
	public Controls.ContextMenu? ContextMenu
	{
		get => GetContextMenu(this);
		set => SetContextMenu(this, value);
	}

	// ── ToolTip attached DP ───────────────────────────────────────────────────

	/// <summary>Attached DP: the tooltip for any <see cref="FrameworkElement"/>.</summary>
	public static readonly DependencyProperty ToolTipProperty =
		Controls.ToolTipService.ToolTipProperty.AddOwner(
			typeof(FrameworkElement), new PropertyMetadata(null, OnToolTipChanged));

	static void OnToolTipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is FrameworkElement fe && fe.IsWidgetCreated)
		{
			// Honour ToolTipService.IsEnabled: a new ToolTip on a suppressed element
			// must not re-show it. Clear the GTK tooltip state when disabled.
			if (Controls.ToolTipService.GetIsEnabled(fe))
				ApplyTooltipToWidget(fe.Widget, e.NewValue);
			else
				ApplyTooltipToWidget(fe.Widget, null);
		}
	}

	internal static void ApplyTooltipToWidget(Gtk.Widget widget, object? tip)
	{
		// Extract the display text. For a ToolTip control, prefer its Content; otherwise ToString().
		string text;
		if (tip is Controls.ToolTip tt)
			text = tt.Content?.ToString() ?? string.Empty;
		else
			text = tip?.ToString() ?? string.Empty;

		widget.TooltipText = text;
		widget.HasTooltip = tip != null;
	}

	/// <summary>
	/// Re-applies or suppresses the tooltip on <paramref name="fe"/>'s widget based on
	/// <see cref="Controls.ToolTipService.IsEnabledProperty"/>. Called when that property changes
	/// on a realized element.
	/// </summary>
	internal static void ReapplyTooltipEnabled(FrameworkElement fe)
	{
		bool enabled = Controls.ToolTipService.GetIsEnabled(fe);
		if (enabled)
		{
			// Restore: re-apply whatever tooltip is set.
			var tip = fe.GetValue(ToolTipProperty);
			ApplyTooltipToWidget(fe.Widget, tip);
		}
		else
		{
			// Suppress: clear GTK tooltip without touching the DP value.
			fe.Widget.TooltipText = string.Empty;
			fe.Widget.HasTooltip = false;
		}
	}

	/// <summary>Gets the tooltip attached to <paramref name="element"/>.</summary>
	public static object? GetToolTip(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return element.GetValue(ToolTipProperty);
	}

	/// <summary>Sets the tooltip on <paramref name="element"/>.</summary>
	public static void SetToolTip(DependencyObject element, object? value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(ToolTipProperty, value);
	}

	/// <summary>Instance accessor: the tooltip on this element.</summary>
	public object? ToolTip
	{
		get => GetToolTip(this);
		set => SetToolTip(this, value);
	}

	// ── Apply deferred tooltip after widget creation ───────────────────────────

	/// <inheritdoc/>
	protected override void OnWidgetCreated(Gtk.Widget widget)
	{
		base.OnWidgetCreated(widget);
		// Map WPF layout properties (size + alignment) onto the new GTK widget.
		ApplyLayoutToWidget(widget);
		var tip = GetValue(ToolTipProperty);
		bool isEnabled = Controls.ToolTipService.GetIsEnabled(this);
		if (tip != null && isEnabled)
			ApplyTooltipToWidget(widget, tip);
		// Apply any Cursor set before widget creation.
		// Implemented in FrameworkElement.Cursor.cs.
		ApplyCursorOnCreated(widget);
		ApplyFlowDirectionOnCreated(widget);
	}

	/// <summary>
	/// Called from <see cref="OnWidgetCreated"/> to apply any
	/// <see cref="Cursor"/> set before widget creation.
	/// Implemented in <c>FrameworkElement.Cursor.cs</c>.
	/// </summary>
	partial void ApplyCursorOnCreated(Gtk.Widget widget);

	/// <summary>Implemented in <c>FrameworkElement.FlowDirection.cs</c>.</summary>
	partial void ApplyFlowDirectionOnCreated(Gtk.Widget widget);
}