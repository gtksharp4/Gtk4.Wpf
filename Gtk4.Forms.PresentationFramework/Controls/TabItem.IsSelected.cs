namespace System.Windows.Controls;

public partial class TabItem
{
	// Selector.IsSelectedProperty does not exist in this layer, so TabItem registers its own; WPF's carries
	// BindsTwoWayByDefault | Journal.
	/// <summary>Identifies <see cref="IsSelected"/>. Default false; binds two-way and journals.</summary>
	public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
		nameof(IsSelected), typeof(bool), typeof(TabItem),
		new FrameworkPropertyMetadata(
			false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
			OnIsSelectedChanged));

	/// <summary>Gets or sets whether this tab is the selected one of its <see cref="TabControl"/>.</summary>
	public bool IsSelected
	{
		get => (bool)GetValue(IsSelectedProperty)!;
		set => SetValue(IsSelectedProperty, value);
	}

	/// <summary>The tab control that hosts this tab, or null.</summary>
	internal TabControl? OwnerTabControl => LogicalParent as TabControl;

	static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var item = (TabItem)d;
		var owner = item.OwnerTabControl;
		if (owner == null) return;
		if ((bool)e.NewValue!)
			owner.SelectedItem = item;
		else if (ReferenceEquals(owner.SelectedItem, item))
			owner.SelectedItem = null;
	}
}
