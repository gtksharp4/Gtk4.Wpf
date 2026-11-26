namespace System.Windows;

/// <summary>
/// Injectable presenter for <see cref="MessageBox"/>. The default implementation
/// shows a real GTK 4 dialog; tests substitute a fake that returns a predetermined
/// result without opening a window.
/// </summary>
public interface IMessageBoxPresenter
{
	/// <summary>Shows the message box and returns the user's choice.</summary>
	MessageBoxResult Show(
		string messageText,
		string caption,
		MessageBoxButton button,
		MessageBoxImage icon);
}