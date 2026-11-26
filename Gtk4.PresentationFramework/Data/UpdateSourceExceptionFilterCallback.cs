namespace System.Windows.Data;

/// <summary>
/// Handles an exception thrown while a binding updates its source. Return <see langword="null"/> for no
/// validation error, a <see cref="System.Windows.Controls.ValidationError"/>, or any object to use as the
/// error content.
/// </summary>
public delegate object UpdateSourceExceptionFilterCallback(object bindExpression, Exception exception);
