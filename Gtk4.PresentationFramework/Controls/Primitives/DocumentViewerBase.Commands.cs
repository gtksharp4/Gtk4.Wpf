using System.Windows.Input;

namespace System.Windows.Controls.Primitives;

public abstract partial class DocumentViewerBase
{
	static DocumentViewerBase()
	{
		RegisterBringIntoView();
		Bind(NavigationCommands.FirstPage, v => v.OnFirstPageCommand(), v => v.CanGoToPreviousPage);
		Bind(NavigationCommands.PreviousPage, v => v.OnPreviousPageCommand(), v => v.CanGoToPreviousPage);
		Bind(NavigationCommands.NextPage, v => v.OnNextPageCommand(), v => v.CanGoToNextPage);
		Bind(NavigationCommands.LastPage, v => v.OnLastPageCommand(), v => v.CanGoToNextPage);
		CommandManager.RegisterClassCommandBinding(typeof(DocumentViewerBase), new CommandBinding(
			NavigationCommands.GoToPage,
			(sender, e) =>
			{
				if (e.Parameter is int n) ((DocumentViewerBase)sender).OnGoToPageCommand(n);
			},
			(sender, e) => e.CanExecute = e.Parameter is int n && ((DocumentViewerBase)sender).CanGoToPage(n)));
	}

	static void Bind(RoutedCommand command, Action<DocumentViewerBase> run, Func<DocumentViewerBase, bool> can) =>
		CommandManager.RegisterClassCommandBinding(typeof(DocumentViewerBase), new CommandBinding(command,
			(sender, _) => run((DocumentViewerBase)sender),
			(sender, e) => e.CanExecute = can((DocumentViewerBase)sender)));
}
