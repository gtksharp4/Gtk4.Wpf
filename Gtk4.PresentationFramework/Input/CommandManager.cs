using System;
using System.Collections.Generic;
using System.Windows;

namespace System.Windows.Input;

/// <summary>
/// Routes <see cref="ICommand.Execute"/> and <see cref="ICommand.CanExecute"/> calls
/// to the nearest matching <see cref="CommandBinding"/> walking up the element tree,
/// and broadcasts <see cref="RequerySuggested"/> so command sources refresh their
/// enabled state.
/// </summary>
public static partial class CommandManager
{
	// ── RequerySuggested — weak-reference handler list (E2) ──────────────────

	// Handlers are stored as WeakReferences so controls that are no longer
	// referenced do not keep themselves alive through this static list.
	static readonly List<WeakReference<EventHandler>> _requeueHandlers =
		new List<WeakReference<EventHandler>>();
	static readonly object _requeueLock = new object();

	/// <summary>
	/// Raised when the state of any command might have changed and all
	/// <see cref="ICommand.CanExecute"/> results should be re-evaluated.
	/// <see cref="RoutedCommand.CanExecuteChanged"/> delegates to this event.
	/// Handlers are held weakly so controls do not root themselves via this list.
	/// </summary>
	public static event EventHandler? RequerySuggested
	{
		add
		{
			if (value == null) return;
			lock (_requeueLock)
				_requeueHandlers.Add(new WeakReference<EventHandler>(value));
		}
		remove
		{
			if (value == null) return;
			lock (_requeueLock)
			{
				for (int i = _requeueHandlers.Count - 1; i >= 0; i--)
				{
					if (_requeueHandlers[i].TryGetTarget(out var h) && ReferenceEquals(h, value))
					{
						_requeueHandlers.RemoveAt(i);
						break;
					}
				}
			}
		}
	}

	/// <summary>
	/// Explicitly raises <see cref="RequerySuggested"/>, causing every bound
	/// command source to refresh its enabled state. Dead weak references are
	/// pruned on each raise.
	/// </summary>
	public static void InvalidateRequerySuggested()
	{
		List<EventHandler> live;
		lock (_requeueLock)
		{
			live = new List<EventHandler>(_requeueHandlers.Count);
			for (int i = _requeueHandlers.Count - 1; i >= 0; i--)
			{
				if (_requeueHandlers[i].TryGetTarget(out var h))
					live.Add(h);
				else
					_requeueHandlers.RemoveAt(i); // prune dead entry
			}
		}
		foreach (var h in live)
			h(null, EventArgs.Empty);
	}

	/// <summary>
	/// Returns the number of live (non-collected) handler registrations.
	/// Exposed for unit testing the pruning behaviour (E2).
	/// </summary>
	internal static int LiveRequeryHandlerCount
	{
		get
		{
			lock (_requeueLock)
			{
				int count = 0;
				for (int i = _requeueHandlers.Count - 1; i >= 0; i--)
				{
					if (_requeueHandlers[i].TryGetTarget(out _))
						count++;
					else
						_requeueHandlers.RemoveAt(i);
				}
				return count;
			}
		}
	}

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
		var binding = FindCommandBinding(command, target, out var owner);
		if (binding != null)
		{
			var args = new CanExecuteRoutedEventArgs(
					CanExecuteEvent, command, parameter)
				{ Source = target };
			binding.InvokeCanExecute(owner!, args);
			return args.CanExecute;
		}
		// A RoutedCommand with no matching binding is not executable (WPF semantics).
		// Delegating to its own CanExecute would re-enter this method → infinite recursion.
		if (command is RoutedCommand) return false;
		// Fallback: delegate to a non-routed command's own implementation.
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
		var binding = FindCommandBinding(command, target, out var owner);
		if (binding != null)
		{
			var args = new ExecutedRoutedEventArgs(ExecutedEvent, command, parameter)
				{ Source = target };
			binding.InvokeExecuted(owner!, args);
			return;
		}
		// A RoutedCommand with no matching binding is a no-op (WPF semantics).
		// Delegating to its own Execute would re-enter this method → infinite recursion.
		if (command is RoutedCommand) return;
		// Fallback: execute a non-routed command directly.
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
	/// E7: walks <see cref="UIElement.LogicalParent"/> (the documented contract),
	/// not <see cref="DependencyObject.InheritanceParent"/>.
	/// </summary>
	static CommandBinding? FindCommandBinding(ICommand command, UIElement? element, out object? owner)
	{
		var current = element;
		while (current != null)
		{
			foreach (var binding in current.CommandBindings)
			{
				if (ReferenceEquals(binding.Command, command))
				{
					owner = element;
					return binding;
				}
			}
			foreach (var binding in ForTypeChain(_classCommandBindings, current.GetType()))
			{
				if (ReferenceEquals(binding.Command, command))
				{
					owner = current;
					return binding;
				}
			}
			current = current.LogicalParent;
		}
		owner = null;
		return null;
	}
}
