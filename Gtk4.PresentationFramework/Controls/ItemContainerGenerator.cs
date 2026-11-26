using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

/// <summary>What an <see cref="ItemContainerGenerator"/> asks of the control that owns the containers.</summary>
interface IContainerHost
{
	GeneratorStatus Status { get; }
	int ItemCount { get; }
	object? ItemAt(int index);
	int IndexOfItem(object item);
	DependencyObject? ContainerAt(int index);

	/// <summary>The index <paramref name="container"/> holds, or -1 when it is not one of the containers.</summary>
	int IndexOfContainer(DependencyObject container);
}

/// <summary>
/// Maps the items of an <see cref="ItemsControl"/> (or of a <see cref="DataGrid"/> or
/// <see cref="DataGridCellsPresenter"/>) to the containers generated for them. An item that is its own container
/// maps to itself.
/// </summary>
public sealed class ItemContainerGenerator
{
	readonly IContainerHost _host;

	internal ItemContainerGenerator(ItemsControl owner) : this(new ItemsControlHost(owner)) { }

	internal ItemContainerGenerator(IContainerHost host) => _host = host;

	/// <summary>Gets the state of the generator.</summary>
	public GeneratorStatus Status => _host.Status;

	/// <summary>Raised when <see cref="Status"/> changes.</summary>
	public event EventHandler? StatusChanged;

	internal void RaiseStatusChanged() => StatusChanged?.Invoke(this, EventArgs.Empty);

	/// <summary>Returns the container of <paramref name="item"/>, or <see langword="null"/>.</summary>
	public DependencyObject? ContainerFromItem(object? item)
	{
		if (item == null) return null;
		return ContainerFromIndex(_host.IndexOfItem(item));
	}

	/// <summary>Returns the container at <paramref name="index"/>, or <see langword="null"/>.</summary>
	public DependencyObject? ContainerFromIndex(int index)
	{
		if (index < 0 || index >= _host.ItemCount) return null;
		return _host.ContainerAt(index);
	}

	/// <summary>Returns the index of <paramref name="container"/>, or -1 when it is not a container.</summary>
	public int IndexFromContainer(DependencyObject container)
	{
		if (container == null) throw new ArgumentNullException(nameof(container));
		return _host.IndexOfContainer(container);
	}

	/// <summary>
	/// Returns the item shown by <paramref name="container"/>, or <see cref="DependencyProperty.UnsetValue"/>
	/// when it is not one of the containers.
	/// </summary>
	public object ItemFromContainer(DependencyObject container)
	{
		if (container == null) throw new ArgumentNullException(nameof(container));
		int index = IndexFromContainer(container);
		return index < 0 ? DependencyProperty.UnsetValue : _host.ItemAt(index)!;
	}

	// The ItemsControl view of the host: rows it generated, or items that are their own container.
	sealed class ItemsControlHost : IContainerHost
	{
		readonly ItemsControl _owner;

		internal ItemsControlHost(ItemsControl owner) => _owner = owner;

		public GeneratorStatus Status => _owner.GeneratorStatusValue;

		public int ItemCount => _owner.Items.Count;

		public object? ItemAt(int index) => _owner.Items[index];

		public int IndexOfItem(object item) => _owner.Items.IndexOf(item);

		public DependencyObject? ContainerAt(int index)
		{
			if (_owner.Items[index] is DependencyObject own && _owner.IsOwnContainer(own)) return own;
			return _owner.ContainerAt(index);
		}

		public int IndexOfContainer(DependencyObject container)
		{
			int row = _owner.IndexOfContainer(container);
			if (row >= 0) return row;
			return _owner.IsOwnContainer(container) ? _owner.Items.IndexOf(container) : -1;
		}
	}
}
