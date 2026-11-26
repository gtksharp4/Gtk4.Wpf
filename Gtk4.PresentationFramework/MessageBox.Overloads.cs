namespace System.Windows;

// Overloads that carry an owner, default result or options. The presenter contract has no slot for
// them, so they are accepted and ignored: GTK shows the same dialog with its own default button.
public static partial class MessageBox
{
	/// <summary>Shows a message box with a default result (ignored on GTK).</summary>
	public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button,
		MessageBoxImage icon, MessageBoxResult defaultResult) =>
		Show(messageBoxText, caption, button, icon);

	/// <summary>Shows a message box with a default result and options (both ignored on GTK).</summary>
	public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button,
		MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options) =>
		Show(messageBoxText, caption, button, icon);

	/// <summary>Shows a message box in front of <paramref name="owner"/> (the owner is ignored on GTK).</summary>
	/// <exception cref="ArgumentNullException"><paramref name="owner"/> is null.</exception>
	public static MessageBoxResult Show(Window owner, string messageBoxText) =>
		Show(messageBoxText, string.Empty, MessageBoxButton.OK, MessageBoxImage.None, RequireOwner(owner));

	/// <summary>Shows a message box with a caption in front of <paramref name="owner"/>.</summary>
	public static MessageBoxResult Show(Window owner, string messageBoxText, string caption) =>
		Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None, RequireOwner(owner));

	/// <summary>Shows a message box with buttons in front of <paramref name="owner"/>.</summary>
	public static MessageBoxResult Show(Window owner, string messageBoxText, string caption,
		MessageBoxButton button) =>
		Show(messageBoxText, caption, button, MessageBoxImage.None, RequireOwner(owner));

	/// <summary>Shows a message box with an icon in front of <paramref name="owner"/>.</summary>
	public static MessageBoxResult Show(Window owner, string messageBoxText, string caption,
		MessageBoxButton button, MessageBoxImage icon) =>
		Show(messageBoxText, caption, button, icon, RequireOwner(owner));

	/// <summary>Shows a message box with a default result in front of <paramref name="owner"/>.</summary>
	public static MessageBoxResult Show(Window owner, string messageBoxText, string caption,
		MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult) =>
		Show(messageBoxText, caption, button, icon, RequireOwner(owner));

	/// <summary>Shows a message box with all options in front of <paramref name="owner"/>.</summary>
	public static MessageBoxResult Show(Window owner, string messageBoxText, string caption,
		MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options) =>
		Show(messageBoxText, caption, button, icon, RequireOwner(owner));

	// Returns the neutral default result so the owner-less overload is reused.
	static MessageBoxResult RequireOwner(Window owner) =>
		owner == null ? throw new ArgumentNullException(nameof(owner)) : MessageBoxResult.None;
}
