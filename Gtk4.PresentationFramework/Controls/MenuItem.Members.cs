namespace System.Windows.Controls;

public partial class MenuItem
{
	/// <summary>Identifies <see cref="IsSubmenuOpen"/>. Default false.</summary>
	public static readonly DependencyProperty IsSubmenuOpenProperty = DependencyProperty.Register(
		nameof(IsSubmenuOpen), typeof(bool), typeof(MenuItem),
		new FrameworkPropertyMetadata(
			false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSubmenuOpenChanged));

	/// <summary>
	/// Identifies <see cref="ItemContainerStyle"/>. MenuItem is not an ItemsControl here, so
	/// it has its own.
	/// </summary>
	public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register(
		nameof(ItemContainerStyle), typeof(Style), typeof(MenuItem), new FrameworkPropertyMetadata(null));

	/// <summary>Gets or sets whether the submenu is open. Opening needs sub-items and a created widget.</summary>
	public bool IsSubmenuOpen
	{
		get => (bool)GetValue(IsSubmenuOpenProperty)!;
		set => SetValue(IsSubmenuOpenProperty, value);
	}

	/// <summary>Gets or sets the style applied to the sub-items. Stored only.</summary>
	public Style? ItemContainerStyle
	{
		get => (Style?)GetValue(ItemContainerStyleProperty);
		set => SetValue(ItemContainerStyleProperty, value);
	}

	/// <inheritdoc/>
	protected override void OnWidgetCreated(Gtk.Widget widget)
	{
		base.OnWidgetCreated(widget);
		if (widget is Gtk.MenuButton { Popover: { } popover })
		{
			popover.Closed += (_, _) => SetValue(IsSubmenuOpenProperty, false);
			if (IsSubmenuOpen && widget.Root != null) popover.Popup();
		}
	}

	static void OnIsSubmenuOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (((MenuItem)d)._gtkMenuButton is not { Root: not null, Popover: { } popover }) return;
		if ((bool)e.NewValue!) popover.Popup();
		else popover.Popdown();
	}
}
