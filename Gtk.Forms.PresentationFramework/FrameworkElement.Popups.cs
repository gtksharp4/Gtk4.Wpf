namespace System.Windows
{
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
			DependencyProperty.RegisterAttached(
				"ToolTip",
				typeof(object),
				typeof(FrameworkElement),
				new PropertyMetadata(null, OnToolTipChanged));

		static void OnToolTipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FrameworkElement fe && fe.IsWidgetCreated)
				ApplyTooltipToWidget(fe.Widget, e.NewValue);
		}

		internal static void ApplyTooltipToWidget(Gtk.Widget widget, object? tip)
		{
			widget.TooltipText = tip?.ToString() ?? string.Empty;
			widget.HasTooltip = tip != null;
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
			var tip = GetValue(ToolTipProperty);
			if (tip != null)
				ApplyTooltipToWidget(widget, tip);
		}
	}
}
