using System.Collections.ObjectModel;

namespace System.Windows.Input;

/// <summary>
/// A strongly-typed collection of <see cref="CommandBinding"/> objects attached to a
/// <see cref="System.Windows.UIElement"/> via <see cref="System.Windows.UIElement.CommandBindings"/>.
/// </summary>
public sealed class CommandBindingCollection : Collection<CommandBinding>
{
}