using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

public partial class ItemsControl
{
	ItemContainerGenerator? _generator;

	/// <summary>Gets the generator that maps the items to their containers.</summary>
	public ItemContainerGenerator ItemContainerGenerator => _generator ??= CreateItemContainerGenerator();

	internal virtual ItemContainerGenerator CreateItemContainerGenerator() => new ItemContainerGenerator(this);

	internal bool IsOwnContainer(object? item) => IsItemItsOwnContainerOverride(item);

	internal int IndexOfContainer(DependencyObject container)
	{
		for (int i = 0; i < _rows.Count; i++)
			if (ReferenceEquals(_rows[i].Container, container)) return i;
		return -1;
	}

	internal GeneratorStatus GeneratorStatusValue { get; private set; }

	void SetGeneratorStatus(GeneratorStatus status)
	{
		if (GeneratorStatusValue == status) return;
		GeneratorStatusValue = status;
		_generator?.RaiseStatusChanged();
	}
}
