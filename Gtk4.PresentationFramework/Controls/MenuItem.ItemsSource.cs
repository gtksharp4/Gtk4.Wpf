using System.Collections;

namespace System.Windows.Controls;

public partial class MenuItem
{
	/// <summary>Identifies the <see cref="ItemsSource"/> dependency property.</summary>
	/// <remarks>
	/// Registered on <see cref="MenuItem"/> itself (WPF shares <c>ItemsControl.ItemsSourceProperty</c>) because
	/// this type is not an <see cref="ItemsControl"/>. Stored only: no items are generated from it.
	/// </remarks>
	public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
		nameof(ItemsSource), typeof(IEnumerable), typeof(MenuItem), new PropertyMetadata(null));

	/// <summary>Gets or sets the collection used to generate the sub-items.</summary>
	public IEnumerable? ItemsSource
	{
		get => (IEnumerable?)GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}
}
