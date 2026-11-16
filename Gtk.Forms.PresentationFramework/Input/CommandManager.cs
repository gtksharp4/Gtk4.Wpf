using System;
using System.Windows;

namespace System.Windows.Input
{
	/// <summary>
	/// Routes <see cref="ICommand.Execute"/> and <see cref="ICommand.CanExecute"/> calls
	/// to the nearest matching <see cref="CommandBinding"/> walking up the element tree,
	/// and broadcasts <see cref="RequerySuggested"/> so command sources refresh their
	/// enabled state.
	/// </summary>
	public static class CommandManager
	{
		// ── RequerySuggested ─────────────────────────────────────────────────────

		/// <summary>
		/// Raised when the state of any command might have changed and all
		/// <see cref="ICommand.CanExecute"/> results should be re-evaluated.
		/// <see cref="RoutedCommand.CanExecuteChanged"/> delegates to this event.
		/// </summary>
		public static event EventHandler? RequerySuggested;

		/// <summary>
		/// Explicitly raises <see cref="RequerySuggested"/>, causing every bound
		/// command source to refresh its enabled state.
		/// </summary>
		public static void InvalidateRequerySuggested() =>
			RequerySuggested?.Invoke(null, EventArgs.Empty);

		// ── Routing helpers ───────────────────────────────────────────────────────

		/// <summary>
		/// Walks <paramref name="target"/>'s logical-parent chain looking for a
		/// <see cref="CommandBinding"/> whose <see cref="CommandBinding.Command"/> matches
		/// <paramref name="command"/>.  Returns <c>true</c> when found and the binding's
		/// <see cref="CommandBinding.CanExecute"/> handler sets
		/// <see cref="CanExecuteRoutedEventArgs.CanExecute"/>.
		/// When no binding is found and <paramref name="command"/> is not a
		/// <see cref="RoutedCommand"/>, falls back to the command's own
		/// <see cref="ICommand.CanExecute"/>.
		/// </summary>
		internal static bool CanExecute(ICommand command, object? parameter, UIElement? target)
		{
			var binding = FindCommandBinding(command, target);
			if (binding != null)
			{
				var args = new CanExecuteRoutedEventArgs(
					CanExecuteEvent, command, parameter)
				{ Source = target };
				binding.InvokeCanExecute(target!, args);
				return args.CanExecute;
			}
			// Fallback: delegate to the command's own implementation.
			return command.CanExecute(parameter);
		}

		/// <summary>
		/// Walks <paramref name="target"/>'s logical-parent chain and invokes the first
		/// matching <see cref="CommandBinding"/>'s <see cref="CommandBinding.Executed"/>
		/// handler.  When no binding is found and <paramref name="command"/> is not a
		/// <see cref="RoutedCommand"/>, calls <see cref="ICommand.Execute"/> directly.
		/// </summary>
		internal static void Execute(ICommand command, object? parameter, UIElement? target)
		{
			var binding = FindCommandBinding(command, target);
			if (binding != null)
			{
				var args = new ExecutedRoutedEventArgs(ExecutedEvent, command, parameter)
				{ Source = target };
				binding.InvokeExecuted(target!, args);
				return;
			}
			// Fallback: execute directly.
			command.Execute(parameter);
		}

		// ── Private helpers ───────────────────────────────────────────────────────

		// Sentinel RoutedEvents used only to construct the event-args objects.
		// They are Direct (not routed through the tree — CommandManager does the walking).
		static readonly RoutedEvent ExecutedEvent =
			EventManager.RegisterRoutedEvent(
				"CommandManager.Executed",
				RoutingStrategy.Direct,
				typeof(ExecutedRoutedEventHandler),
				typeof(CommandManager));

		static readonly RoutedEvent CanExecuteEvent =
			EventManager.RegisterRoutedEvent(
				"CommandManager.CanExecute",
				RoutingStrategy.Direct,
				typeof(CanExecuteRoutedEventHandler),
				typeof(CommandManager));

		/// <summary>
		/// Walks up from <paramref name="element"/> through the logical-parent chain and
		/// returns the first <see cref="CommandBinding"/> whose command reference-equals
		/// <paramref name="command"/>, or <c>null</c> when none is found.
		/// </summary>
		static CommandBinding? FindCommandBinding(ICommand command, UIElement? element)
		{
			var current = element;
			while (current != null)
			{
				foreach (var binding in current.CommandBindings)
				{
					if (ReferenceEquals(binding.Command, command))
						return binding;
				}
				// Walk up via LogicalParent (accessible via InheritanceParent cast).
				current = current.InheritanceParent as UIElement;
			}
			return null;
		}
	}
}
