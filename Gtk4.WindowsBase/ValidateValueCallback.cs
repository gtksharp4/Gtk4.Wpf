namespace System.Windows;

/// <summary>
/// Returns <c>false</c> for a value that is not valid for a <see cref="DependencyProperty"/>.
/// </summary>
public delegate bool ValidateValueCallback(object? value);
