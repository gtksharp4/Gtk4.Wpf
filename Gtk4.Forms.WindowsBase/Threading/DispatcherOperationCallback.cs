namespace System.Windows.Threading;

/// <summary>Represents a method that takes one argument and returns an object, run by a <see cref="Dispatcher"/>.</summary>
/// <param name="arg">The argument passed to the method.</param>
public delegate object DispatcherOperationCallback(object arg);
