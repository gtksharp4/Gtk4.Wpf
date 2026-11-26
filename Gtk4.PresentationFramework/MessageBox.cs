namespace System.Windows;

/// <summary>
/// Displays a modal message box. All overloads are static and synchronous, matching
/// the WPF API. The actual presentation is delegated to <see cref="Presenter"/> so
/// tests can substitute a fake without opening a real window.
/// </summary>
/// <remarks>
/// <b>GTK4 async caveat:</b> GTK 4 removed the synchronous <c>gtk_dialog_run</c>. The
/// default <see cref="GtkMessageBoxPresenter"/> recovers synchrony by running a nested
/// <see cref="GLib.MainLoop"/>. In a non-interactive (headless) session that loop blocks
/// indefinitely because no user dismisses the dialog — set <see cref="Presenter"/> to a
/// <see cref="FakeMessageBoxPresenter"/> (or any <see cref="IMessageBoxPresenter"/>) before
/// calling <c>Show</c> in tests or automated environments.
/// </remarks>
public static partial class MessageBox
{
	/// <summary>
	/// Injectable presenter. Defaults to <see cref="GtkMessageBoxPresenter"/>;
	/// replace with a test double in unit tests.
	/// </summary>
	public static IMessageBoxPresenter Presenter { get; set; } =
		new GtkMessageBoxPresenter();

	/// <summary>Shows a message box with the given text.</summary>
	public static MessageBoxResult Show(string messageBoxText) =>
		Show(messageBoxText, string.Empty, MessageBoxButton.OK, MessageBoxImage.None);

	/// <summary>Shows a message box with text and caption.</summary>
	public static MessageBoxResult Show(string messageBoxText, string caption) =>
		Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None);

	/// <summary>Shows a message box with text, caption, and button set.</summary>
	public static MessageBoxResult Show(
		string messageBoxText,
		string caption,
		MessageBoxButton button) =>
		Show(messageBoxText, caption, button, MessageBoxImage.None);

	/// <summary>Shows a message box with all options.</summary>
	public static MessageBoxResult Show(
		string messageBoxText,
		string caption,
		MessageBoxButton button,
		MessageBoxImage icon) =>
		Presenter.Show(messageBoxText ?? string.Empty, caption ?? string.Empty, button, icon);
}