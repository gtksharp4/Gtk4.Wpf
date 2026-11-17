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
			new PropertyMetadata(null));

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
