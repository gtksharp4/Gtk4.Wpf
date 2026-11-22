using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

/// <summary>
/// Maps the items of an <see cref="ItemsControl"/> to the containers it generated for them. Backed by the
/// control's rows; an item that is its own container maps to itself.
/// </summary>
public sealed class ItemContainerGenerator
{
	readonly ItemsControl _owner;

	internal ItemContainerGenerator(ItemsControl owner) => _owner = owner;

	/// <summary>Gets the state of the generator.</summary>
	public GeneratorStatus Status => _owner.GeneratorStatusValue;

	/// <summary>Raised when <see cref="Status"/> changes.</summary>
	public event EventHandler? StatusChanged;

	internal void RaiseStatusChanged() => StatusChanged?.Invoke(this, EventArgs.Empty);

	/// <summary>Returns the container of <paramref name="item"/>, or <see langword="null"/>.</summary>
	public DependencyObject? ContainerFromItem(object? item)
	{
		if (item == null) return null;
		return ContainerFromIndex(_owner.Items.IndexOf(item));
	}

	/// <summary>Returns the container at <paramref name="index"/>, or <see langword="null"/>.</summary>
	public DependencyObject? ContainerFromIndex(int index)
	{
		if (index < 0 || index >= _owner.Items.Count) return null;
		if (_owner.Items[index] is DependencyObject own && _owner.IsOwnContainer(own)) return own;
		return _owner.ContainerAt(index);
	}

	/// <summary>Returns the index of <paramref name="container"/>, or -1 when it is not a container.</summary>
	public int IndexFromContainer(DependencyObject container)
	{
		if (container == null) return -1;
		int row = _owner.IndexOfContainer(container);
		if (row >= 0) return row;
		return _owner.IsOwnContainer(container) ? _owner.Items.IndexOf(container) : -1;
	}

	/// <summary>
	/// Returns the item shown by <paramref name="container"/>, or <see cref="DependencyProperty.UnsetValue"/>
	/// when it is not one of the containers.
	/// </summary>
	public object ItemFromContainer(DependencyObject container)
	{
		int index = IndexFromContainer(container);
		return index < 0 ? DependencyProperty.UnsetValue : _owner.Items[index]!;
	}
}
