using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Gtk4.PresentationFramework.Tests.Lifecycle;

/// <summary>
/// Replaces <see cref="Dispatcher.BeginInvokeAction"/> with a manual queue for the lifetime of
/// the instance so tests can observe that deferred (<see cref="DispatcherPriority.Loaded"/>)
/// work does not run synchronously, then pump it explicitly.
/// </summary>
sealed class ManualDispatcherQueue : IDisposable
{
	readonly Action<Action> _previous;
	readonly Action<Action, DispatcherPriority>? _previousPriority;
	readonly Queue<Action> _queue = new Queue<Action>();

	public ManualDispatcherQueue()
	{
		_previous = Dispatcher.BeginInvokeAction;
		_previousPriority = Dispatcher.BeginInvokePriorityAction;
		Dispatcher.BeginInvokePriorityAction = null;
		Dispatcher.BeginInvokeAction = a => _queue.Enqueue(a);
	}

	public int Pending => _queue.Count;

	/// <summary>Runs queued work (including work queued while pumping) until empty.</summary>
	public void Pump()
	{
		while (_queue.Count > 0)
			_queue.Dequeue()();
	}

	public void Dispose()
	{
		Dispatcher.BeginInvokeAction = _previous;
		Dispatcher.BeginInvokePriorityAction = _previousPriority;
	}
}

/// <summary>Builds named trees and records lifecycle events in order.</summary>
static class LifecycleTree
{
	/// <summary>root(StackPanel) → [inner(StackPanel) → [grand(Border)], sibling(Border)].</summary>
	public static StackPanel Build(out StackPanel inner, out Border grand, out Border sibling)
	{
		var root = new StackPanel { Name = "root" };
		inner = new StackPanel { Name = "inner" };
		grand = new Border { Name = "grand" };
		sibling = new Border { Name = "sibling" };
		inner.Children.Add(grand);
		root.Children.Add(inner);
		root.Children.Add(sibling);
		return root;
	}

	public static void Record(List<string> log, string prefix, params FrameworkElement[] elements)
	{
		foreach (var fe in elements)
		{
			var name = fe.Name;
			fe.Loaded += (_, _) => log.Add(prefix + "Loaded:" + name);
			fe.Unloaded += (_, _) => log.Add(prefix + "Unloaded:" + name);
		}
	}
}
