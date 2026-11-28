using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace System.Windows.Input;

public static partial class CommandManager
{
	// A TextBox answers the editing commands itself, as in WPF, and a selection change suggests a requery
	// (WPF's TextEditor does the same), so a Copy button follows the selection.
	static void HookTextEditing()
	{
		Register(ApplicationCommands.Copy, t => t.SelectionLength > 0, t => t.Copy());
		Register(ApplicationCommands.Cut, t => t.SelectionLength > 0 && !t.IsReadOnly, t => t.Cut());
		Register(ApplicationCommands.Paste, t => !t.IsReadOnly, t => t.Paste());
		Register(ApplicationCommands.SelectAll, t => true, t => t.SelectAll());
		EventManager.RegisterClassHandler(typeof(TextBoxBase), TextBoxBase.SelectionChangedEvent,
			new RoutedEventHandler(OnInputRequery), true);
	}

	static void Register(RoutedCommand command, Func<TextBox, bool> canExecute, Action<TextBox> execute) =>
		RegisterClassCommandBinding(typeof(TextBox), new CommandBinding(command,
			(sender, _) => execute((TextBox)sender),
			(sender, e) => e.CanExecute = canExecute((TextBox)sender)));
}
