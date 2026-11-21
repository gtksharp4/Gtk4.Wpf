namespace System.Windows.Controls;

public partial class ItemsControl
{
	/// <summary>Returns the item container that holds <paramref name="element"/>, or null when it is not inside one.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="element"/> is null.</exception>
	public DependencyObject? ContainerFromElement(DependencyObject element) => ContainerFromElement(this, element);

	/// <summary>Returns the container of <paramref name="itemsControl"/> that holds <paramref name="element"/>.</summary>
	/// <exception cref="ArgumentNullException">An argument is null.</exception>
	public static DependencyObject? ContainerFromElement(ItemsControl itemsControl, DependencyObject element)
	{
		if (itemsControl == null) throw new ArgumentNullException(nameof(itemsControl));
		if (element == null) throw new ArgumentNullException(nameof(element));
		DependencyObject? current = element;
		while (current != null)
		{
			var parent = (current as UIElement)?.LogicalParent ?? current.InheritanceParent;
			if (ReferenceEquals(parent, itemsControl))
				return current;
			current = parent;
		}
		return null;
	}
}
