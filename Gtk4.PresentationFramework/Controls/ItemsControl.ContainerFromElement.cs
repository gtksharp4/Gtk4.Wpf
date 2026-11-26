namespace System.Windows.Controls;

public partial class ItemsControl
{
	/// <summary>Returns the item container that holds <paramref name="element"/>, or null when it is not inside one.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="element"/> is null.</exception>
	public DependencyObject? ContainerFromElement(DependencyObject element) => ContainerFromElement(this, element);

	/// <summary>
	/// Returns the item container that holds <paramref name="element"/>: of <paramref name="itemsControl"/> when
	/// given, otherwise of whichever <see cref="ItemsControl"/> the element sits in.
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="element"/> is null.</exception>
	public static DependencyObject? ContainerFromElement(ItemsControl? itemsControl, DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		DependencyObject? current = element;
		while (current != null)
		{
			var parent = (current as UIElement)?.LogicalParent ?? current.InheritanceParent;
			var owner = parent as ItemsControl;
			if (owner != null && (itemsControl == null || ReferenceEquals(owner, itemsControl))
				&& owner.IsItemContainer(current))
				return current;
			current = parent;
		}
		return null;
	}

	bool IsItemContainer(DependencyObject candidate)
	{
		foreach (var row in _rows)
			if (ReferenceEquals(row.Container, candidate)) return true;
		return candidate is UIElement && Items.Contains(candidate) && IsItemItsOwnContainerOverride(candidate);
	}
}
