using System.Collections.ObjectModel;

namespace System.Windows.Input;

/// <summary>
/// An ordered collection of <see cref="InputGesture"/> objects associated with
/// a <see cref="RoutedCommand"/>.
/// </summary>
public sealed class InputGestureCollection : Collection<InputGesture>
{
}