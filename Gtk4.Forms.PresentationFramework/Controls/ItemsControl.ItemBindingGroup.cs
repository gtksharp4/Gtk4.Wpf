using System.Windows.Data;

namespace System.Windows.Controls;

public partial class ItemsControl
{
	/// <summary>Identifies the <see cref="ItemBindingGroup"/> dependency property.</summary>
	public static readonly DependencyProperty ItemBindingGroupProperty =
		DependencyProperty.Register(nameof(ItemBindingGroup), typeof(BindingGroup), typeof(ItemsControl),
			new PropertyMetadata(null, (d, _) => ((ItemsControl)d).RebuildItems()));

	/// <summary>
	/// Gets or sets the group copied onto every generated item container as its
	/// <see cref="FrameworkElement.BindingGroup"/> (name, flags and rules; the rule instances are shared).
	/// </summary>
	public BindingGroup? ItemBindingGroup
	{
		get => (BindingGroup?)GetValue(ItemBindingGroupProperty);
		set => SetValue(ItemBindingGroupProperty, value);
	}

	void ApplyItemBindingGroup(UIElement container)
	{
		if (ItemBindingGroup is { } group && container is FrameworkElement element)
			element.BindingGroup = group.CloneForItem();
	}
}
