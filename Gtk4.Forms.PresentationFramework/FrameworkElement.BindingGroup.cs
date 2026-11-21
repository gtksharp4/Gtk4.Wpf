using System.Windows.Data;

namespace System.Windows;

public partial class FrameworkElement
{
	/// <summary>
	/// Identifies the <see cref="BindingGroup"/> dependency property.
	/// </summary>
	public static readonly DependencyProperty BindingGroupProperty =
		DependencyProperty.Register(
			nameof(BindingGroup),
			typeof(BindingGroup),
			typeof(FrameworkElement),
			new PropertyMetadata(null, OnBindingGroupChanged));

	static void OnBindingGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var element = (FrameworkElement)d;
		if (e.OldValue is BindingGroup oldGroup) BindingGroupEnlister.WithdrawSubtree(element, oldGroup);
		if (e.NewValue is BindingGroup) BindingGroupEnlister.EnlistSubtree(element);
	}

	// A subtree that moves under (or out from) a group joins (or leaves) it.
	internal override void OnLogicalParentChangedBindingGroup(UIElement? oldParent, UIElement? newParent)
	{
		if (oldParent != null && FindBindingGroup(oldParent) is { } old)
			BindingGroupEnlister.WithdrawSubtree(this, old);
		if (newParent != null) BindingGroupEnlister.EnlistSubtree(this);
	}

	static BindingGroup? FindBindingGroup(UIElement start)
	{
		for (var e = start; e != null; e = e.LogicalParent)
			if (e is FrameworkElement fe && fe.BindingGroup is { } group) return group;
		return null;
	}

	/// <summary>
	/// Gets or sets the <see cref="BindingGroup"/> used for transactional
	/// validation and commit of bindings on this element.
	/// </summary>
	public BindingGroup? BindingGroup
	{
		get => (BindingGroup?)GetValue(BindingGroupProperty);
		set => SetValue(BindingGroupProperty, value);
	}
}
