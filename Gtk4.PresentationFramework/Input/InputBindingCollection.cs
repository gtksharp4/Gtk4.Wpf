using System.Collections.ObjectModel;

namespace System.Windows.Input;

/// <summary>
/// A strongly-typed collection of <see cref="InputBinding"/> objects attached to a
/// <see cref="System.Windows.UIElement"/> via <see cref="System.Windows.UIElement.InputBindings"/>.
/// </summary>
public sealed class InputBindingCollection : Collection<InputBinding>
{
}